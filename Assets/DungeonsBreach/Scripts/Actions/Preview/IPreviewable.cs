using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPreviewable<T>
{
    public IPreviewable<T> GeneratePreview(T data);
    public IEnumerator StartPreview();
    public void StopPreview();
}


public class UnitDamagePreviewData
{
    public AnimationStateData[] animationData;
    public UnitHealthBar unitHealthBar;
}