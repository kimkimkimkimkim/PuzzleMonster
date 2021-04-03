using System.Collections.Generic;
using GameBase;
using UnityEngine;

[ResourcePath("UI/Parts/Parts-MonsterBoxScrollItem")]
public class MonsterBoxScrollItem : ScrollItem
{
    [SerializeField] protected List<GameObject> _gradeStarOnImageBaseList;

    public void SetGradeImage(int grade) {
        if (grade < 0 || _gradeStarOnImageBaseList.Count < grade) return;
        _gradeStarOnImageBaseList.ForEach((b, index) => {
            b.SetActive(index <= grade - 1);
        });
    }
}