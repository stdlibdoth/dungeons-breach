using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPreviewable<T>
{
    public PreviewKey PreviewKey{get;set;}
    public IPreviewable<T> GeneratePreview(T data);
    public ActionTileInfo[] StartPreview();
    public void StopDamagePreview();
}


public class UnitDamagePreviewData
{
    public SpriteRenderer spriteRenderer;
    public UnitHealthBar unitHealthBar;
}