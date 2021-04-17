using System.Collections.Generic;
using UnityEngine;
using GameBase;

public class MonsterGradeParts : MonoBehaviour
{
    [SerializeField] protected List<GameObject> _gradeStarOnImageBaseList;

    public void SetGradeImage(int grade)
    {
        if (grade < 0 || _gradeStarOnImageBaseList.Count < grade) return;
        _gradeStarOnImageBaseList.ForEach((b, index) => {
            b.SetActive(index <= grade - 1);
        });
    }
}
