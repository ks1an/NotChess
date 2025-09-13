using System.Collections.Generic;
using UnityEngine;

public sealed class StackView : MonoBehaviour
{
    [SerializeField] float objHight;
    [SerializeField] Vector3 objRotInStack;
    [SerializeField] Vector2 randomPosX, randomPosY, RandomPosZ;
    [SerializeField] Vector2 randomRotX, randomRotY, RandomRotZ;
    Stack<Transform> objStack = new();
    int countInStack;
    readonly MathOperations rnd = MathOperations.GetInstance();

    public void Add(GameObject cardBackPrefab, int countToAdd = 1)
    {
        for(int i = 0; i < countToAdd; i++)
        {
            Transform objT = Instantiate(cardBackPrefab, gameObject.transform).transform;
            objT.SetLocalPositionAndRotation(new Vector3(0, countInStack * objHight, 0) + new Vector3(
                    rnd.GetSafeRandom(randomPosX[0], randomPosX[1]),
                    rnd.GetSafeRandom(randomPosY[0], randomPosY[1]),
                    rnd.GetSafeRandom(RandomPosZ[0], RandomPosZ[1])), 
                Quaternion.Euler(objRotInStack + new Vector3(
                    rnd.GetSafeRandom(randomRotX[0], randomRotX[1]),
                    rnd.GetSafeRandom(randomRotY[0], randomRotY[1]),
                    rnd.GetSafeRandom(RandomRotZ[0], RandomRotZ[1])
                    )));

            objStack.Push(objT);
            countInStack++;
        }
    }

    public void Remove(int countToRemove = 1)
    {
        for(int i = 0; i < countToRemove; i++)
        {
            if (countInStack < 1)
                break;

            Destroy(objStack.Pop().gameObject);
            countInStack--;
        }
    }

    public void RemoveAll() => Remove(countInStack);

    public int GetCountInStack() { return countInStack; }
}
