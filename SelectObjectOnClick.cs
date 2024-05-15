using System.Collections.Generic;
using UnityEngine;

public class SelectObjectOnClick : MonoBehaviour
{
    [SerializeField] GameObject Stone;
    [SerializeField] AudioClip[] Se;
    [SerializeField] GameObject highlightPrefab; // ハイライトプレハブ

    AudioSource Asrc;
    int TurnNumber = 0;

    int[][] GameMatrix;
    List<GameObject> stones = new List<GameObject>(); // 配置された駒のリスト
    GameObject lastPlacedStone = null; // 最後に配置された駒
    private bool flipped;

    private void Start()
    {
        Asrc = GetComponent<AudioSource>();

        GameMatrix = new int[8][];
        for (int i = 0; i < 8; i++)
        {
            GameMatrix[i] = new int[8];
            for (int j = 0; j < 8; j++)
            {
                GameMatrix[i][j] = -1;
            }
        }

        // 初期配置
        SetInitialStone(3, 3, 0);
        SetInitialStone(4, 4, 0);
        SetInitialStone(3, 4, 1);
        SetInitialStone(4, 3, 1);
    }

    private void SetInitialStone(int v, int h, int player)
    {
        GameMatrix[v][h] = player;
        Vector3 position = GetPositionFromIndices(v, h);
        float zz = player == 1 ? 180f : 0;
        GameObject initialStone = Instantiate(Stone, position, Quaternion.Euler(0, 0, zz));
        initialStone.transform.localScale *= 0.8f;
        stones.Add(initialStone);
    }

    private Vector3 GetPositionFromIndices(int v, int h)
    {
        // インデックスからワールド座標への変換 (適切な変換を実装する必要があります)
        // これは仮の実装で、正しい位置を返すように調整する必要があります
        return new Vector3(v, 0, h);
    }

    private int getValue(int v, int h)
    {
        return GameMatrix[v][h];
    }

    private void setValue(int v, int h, int value)
    {
        GameMatrix[v][h] = value;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject selectedObject = hit.transform.gameObject;
                MyBlocks mb = selectedObject.GetComponent<MyBlocks>();
                if (mb)
                {
                    Debug.Log("---> V:" + mb.Vertical + " H:" + mb.Horizon);

                    int currentPlayer = TurnNumber % 2;
                    if (IsValidMove(mb.Vertical, mb.Horizon, currentPlayer))
                    {
                        Asrc.clip = Se[0];

                        TurnNumber++;
                        float zz = currentPlayer == 1 ? 180f : 0;
                        GameObject lastStone = Instantiate(Stone, selectedObject.transform.position, selectedObject.transform.rotation * Quaternion.Euler(0, 0, zz));
                        lastStone.transform.localScale *= 0.8f;
                        setValue(mb.Vertical, mb.Horizon, currentPlayer);

                        FlipPieces(mb.Vertical, mb.Horizon, currentPlayer);

                        // 最後の駒のハイライト
                        if (lastPlacedStone != null)
                        {
                            Destroy(lastPlacedStone);
                        }

                        stones.Add(lastStone);
                    }
                    else
                    {
                        Asrc.clip = Se[1];
                    }
                    Asrc.Play();
                    DebugMatrix();

                    if (IsGameOver())
                    {
                        Debug.Log("ゲームオーバー！");
                    }
                }
            }
        }
    }

    bool IsValidMove(int v, int h, int player)
    {
        if (getValue(v, h) != -1)
            return false;
        return CheckDirection(v, h, player, -1, 0) || CheckDirection(v, h, player, 1, 0) ||
               CheckDirection(v, h, player, 0, -1) || CheckDirection(v, h, player, 0, 1) ||
               CheckDirection(v, h, player, -1, -1) || CheckDirection(v, h, player, 1, 1) ||
               CheckDirection(v, h, player, -1, 1) || CheckDirection(v, h, player, 1, -1);
    }

    bool CheckDirection(int v, int h, int player, int dirV, int dirH)
    {
        int opponent = 1 - player;
        bool foundOpponent = false;
        int i = v + dirV, j = h + dirH;
        while (i >= 0 && i < 8 && j >= 0 && j < 8)
        {
            if (getValue(i, j) == opponent)
            {
                foundOpponent = true;
            }
            else if (getValue(i, j) == player)
            {
                return foundOpponent;
            }
            else
            {
                break;
            }
            i += dirV;
            j += dirH;
        }
        return false;
    }

    void FlipPieces(int v, int h, int player)
    {
        if (CheckDirection(v, h, player, -1, 0)) FlipDirection(v, h, player, -1, 0);
        if (CheckDirection(v, h, player, 1, 0)) FlipDirection(v, h, player, 1, 0);
        if (CheckDirection(v, h, player, 0, -1)) FlipDirection(v, h, player, 0, -1);
        if (CheckDirection(v, h, player, 0, 1)) FlipDirection(v, h, player, 0, 1);
        if (CheckDirection(v, h, player, -1, -1)) FlipDirection(v, h, player, -1, -1);
        if (CheckDirection(v, h, player, 1, 1)) FlipDirection(v, h, player, 1, 1);
        if (CheckDirection(v, h, player, -1, 1)) FlipDirection(v, h, player, -1, 1);
        if (CheckDirection(v, h, player, 1, -1)) FlipDirection(v, h, player, 1, -1);
    }

    void FlipDirection(int v, int h, int player, int dirV, int dirH)
    {
        int opponent = 1 - player;
        int i = v + dirV, j = h + dirH;
        while (i >= 0 && i < 8 && j >= 0 && j < 8 && getValue(i, j) == opponent)
        {
            setValue(i, j, player);
            if (stones != null && stones.Count > 0) // stonesがnullでないことを確認し、nullでない場合にのみ処理を実行
            {
                foreach (GameObject stone in stones)
                {
                    MyBlocks mb = stone.GetComponent<MyBlocks>();
                    if (mb != null && mb.Vertical == i && mb.Horizon == j) // nullチェックを追加
                    {
                        float zz = player == 1 ? 0f : 180f;
                        stone.transform.rotation *= Quaternion.Euler(0, 0, zz);
                        flipped = true;
                        break;
                    }
                }
            }
            i += dirV;
            j += dirH;
        }
        if (flipped)
        {
            lastPlacedStone = stones[stones.Count - 1];
        }
    }


    bool IsGameOver()
    {
        for (int v = 0; v < 8; v++)
        {
            for (int h = 0; h < 8; h++)
            {
                if (getValue(v, h) == -1 && (IsValidMove(v, h, 0) || IsValidMove(v, h, 1)))
                {
                    return false;
                }
            }
        }
        return true;
    }

    void DebugMatrix()
    {
        foreach (int[] m in GameMatrix)
        {
            string outStr = "";
            foreach (int m2 in m)
                outStr += "" + m2;

            Debug.Log("----->" + outStr);
        }
    }
}
