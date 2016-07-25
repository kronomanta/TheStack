using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TheStack : MonoBehaviour
{
    public GameObject endPanel;
    public Text scoreText;
    public Color32[] gameColors = new Color32[4];
    public Material stackMat;


    private const float BOUNDS_SIZE = 3.5f;
    private const float STACK_MOVING_SPEED = 5.0f;
    private const float ERROR_MARGIN = 0.1f;
    private const float STACK_BOUNDS_GAIN = 0.25f;
    private const int COMBO_START_GAIN = 3;


    private Transform[] theStack;
    private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

    private int scoreCount = 0;
    private int stackIndex;
    private float combo = 0;

    private float tileTransition = 0f;
    private float tileSpeed = 2.5f;
    private float secondaryPosition;

    private bool isMovingOnX = true;
    private bool isGameOver = false;

    private Vector3 desiredPosition;
    private Vector3 lastTilePosition;

    void Start()
    {
        theStack = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i);
            ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
        }

        stackIndex = theStack.Length - 1;
    }

    /// <summary>
    /// Create a rubble after a not perfect placement
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="scale"></param>
    void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();

        go.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(go.GetComponent<MeshFilter>().mesh);
    }


    void Update()
    {
        //stop the game
        if (isGameOver) return;

        //check left mouse button or touch 
        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceTile())
            {
                SpawnTile();
                scoreCount++;
                scoreText.text = scoreCount.ToString();
            }
            else
            {
                EndGame();
            }
        }

        MoveTile();

        //move the stack lower
        transform.position = Vector3.Lerp(transform.position, desiredPosition, STACK_MOVING_SPEED * Time.deltaTime);
    }

    void MoveTile()
    {
        tileTransition += Time.deltaTime * tileSpeed;
        Vector3 dir;
        if (isMovingOnX)
            dir = new Vector3(Mathf.Sin(tileTransition) * BOUNDS_SIZE, scoreCount, secondaryPosition);
        else
            dir = new Vector3(secondaryPosition, scoreCount, Mathf.Sin(tileTransition) * BOUNDS_SIZE);

        theStack[stackIndex].localPosition = dir;
    }

    /// <summary>
    /// Lost the game, deal with that
    /// </summary>
    void EndGame()
    {
        if (PlayerPrefs.GetInt("score") < scoreCount)
            PlayerPrefs.SetInt("score", scoreCount);

        isGameOver = true;
        endPanel.SetActive(true);
        theStack[stackIndex].gameObject.AddComponent<Rigidbody>();
    }

    /// <summary>
    /// Place the new tile on the top of the stack
    /// </summary>
    void SpawnTile()
    {
        lastTilePosition = theStack[stackIndex].localPosition;

        stackIndex--;
        if (stackIndex < 0)
            stackIndex = theStack.Length - 1;

        desiredPosition = Vector3.down * scoreCount;

        Transform t = theStack[stackIndex];

        t.localPosition = new Vector3(0, scoreCount, 0);
        t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
        ColorMesh(t.GetComponent<MeshFilter>().mesh);
    }

    bool PlaceTile()
    {
        Transform t = theStack[stackIndex];

        if (isMovingOnX)
        {
            float deltaAbs = Mathf.Abs(lastTilePosition.x - t.position.x);
            if (deltaAbs > ERROR_MARGIN)
            {
                //CUT THE TILE
                combo = 0;
                stackBounds.x -= deltaAbs;
                if (stackBounds.x <= 0)
                    return false;

                float middle = lastTilePosition.x + t.localPosition.x / 2;
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

                CreateRubble(
                    new Vector3(t.position.x + t.localScale.x / 2 * Mathf.Sign(t.position.x), t.position.y, t.position.z),
                    new Vector3(deltaAbs, 1, t.localScale.z)
                    );

                t.localPosition = new Vector3(middle - lastTilePosition.x / 2, scoreCount, lastTilePosition.z);
            }
            else
            {
                if (combo > COMBO_START_GAIN)
                {
                    stackBounds.x += STACK_BOUNDS_GAIN;
                    if (stackBounds.x > BOUNDS_SIZE)
                        stackBounds.x = BOUNDS_SIZE;

                    float middle = lastTilePosition.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(middle - lastTilePosition.x / 2, scoreCount, lastTilePosition.z);
                }

                combo++;
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
            }
        }
        else
        {
            float deltaAbs = Mathf.Abs(lastTilePosition.z - t.position.z);
            if (deltaAbs > ERROR_MARGIN)
            {
                //CUT THE TILE
                combo = 0;
                stackBounds.y -= deltaAbs;
                if (stackBounds.y <= 0)
                    return false;

                float middle = lastTilePosition.z + t.localPosition.z / 2;
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

                CreateRubble(
                    new Vector3(t.position.x, t.position.y, t.position.z + t.localScale.z / 2 * Mathf.Sign(t.position.z)),
                    new Vector3(t.localScale.x, 1, deltaAbs)
                    );

                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - lastTilePosition.z / 2);
            }
            else
            {
                if (combo > COMBO_START_GAIN)
                {
                    stackBounds.y += STACK_BOUNDS_GAIN;
                    if (stackBounds.y > BOUNDS_SIZE)
                        stackBounds.y = BOUNDS_SIZE;

                    float middle = lastTilePosition.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - lastTilePosition.z / 2);
                }

                combo++;
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
            }
        }

        secondaryPosition = isMovingOnX
            ? t.localPosition.x
            : t.localPosition.z;

        isMovingOnX = !isMovingOnX;
        return true;
    }

    void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        float f = Mathf.Sin(scoreCount*0.25f);

        for (int i = 0; i < vertices.Length; i++)
            colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);

        mesh.colors32 = colors;
    }

    Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < 0.33f)
            return Color.Lerp(a, b, t / 0.33f);

        if (t < 0.66f)
            return Color.Lerp(b, c, (t - 0.33f) / 0.33f);

        return Color.Lerp(c, d, (t - 0.66f) / 0.66f);
    }

    /// <summary>
    /// Change the scene
    /// </summary>
    /// <param name="sceneName"></param>
    public void OnButtonClick(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);   
    }
}
