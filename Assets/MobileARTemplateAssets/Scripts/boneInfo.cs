using UnityEngine;

[CreateAssetMenu(fileName = "NewBone", menuName = "Bone")]

public class BoneInfo : ScriptableObject
{
    [SerializeField] public string boneName;
    [SerializeField] public int boneID;
    [TextArea(1, 10)]
    [SerializeField] public string boneDescription;
    [SerializeField] public string boneType;
    [SerializeField] public string link;
}