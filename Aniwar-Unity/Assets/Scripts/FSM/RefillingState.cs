using System.Collections;
using UnityEngine;

public class RefillingState : IBoardState
{
    private GridManager board;

    public RefillingState(GridManager board)
    {
        this.board = board;
    }

    public void Enter()
    {
        board.EnableInput(false);
        board.StartCoroutine(RefillCoroutine());
    }

    public void Execute() { }
    public void Exit() { }

    private IEnumerator RefillCoroutine()
    {
        yield return new WaitForSeconds(0.1f);

        for (int x = 0; x < board._width; x++)
        {
            int targetY = board._height - 1;

            // 1️⃣ Dồn gem sống lên TRÊN
            for (int y = board._height - 1; y >= 0; y--)
            {
                GameObject gemObj = board._allGems[x, y];
                if (gemObj == null) continue;

                if (y != targetY)
                {
                    board._allGems[x, targetY] = gemObj;
                    board._allGems[x, y] = null;

                    Gem gem = gemObj.GetComponent<Gem>();
                    gem.SetGridPosition(x, targetY);

                    Vector2 targetPos = board.GetWorldPosition(x, targetY);
                    board.StartCoroutine(
                        board.MoveGemToPosition(gemObj, targetPos)
                    );
                }

                targetY--;
            }

            // 2️⃣ Spawn gem mới lấp CHỖ TRỐNG Ở DƯỚI
            for (int y = targetY; y >= 0; y--)
            {
                Vector2 spawnPos = board.GetWorldPosition(x, y - (targetY + 2));
                Vector2 targetPos = board.GetWorldPosition(x, y);

                GameObject newGem = Object.Instantiate(
                    board.GetRandomGemPrefab(),
                    spawnPos,
                    Quaternion.identity,
                    board.transform
                );

                Gem gem = newGem.GetComponent<Gem>();
                gem.SetGridPosition(x, y);
                gem.SetMatched(false);

                board._allGems[x, y] = newGem;

                board.StartCoroutine(
                    board.MoveGemToPosition(newGem, targetPos)
                );
            }
        }

        // 3️⃣ Đợi animation xong
        yield return new WaitForSeconds(1f);

        // 4️⃣ Check combo
        board.StateManager.ChangeState(
            new CheckingState(board)
        );
    }
}
