using UnityEngine;

namespace ReWorkflow.Components;

public class EventGridPatchBehaviour : MonoBehaviour
{
   public GridChild RefChild;
   private GridChild _gridChild;
   private int _localSize;
   private Vector3 _originalOffset;

   private void Awake()
   {
      _gridChild = GetComponent<GridChild>();
      _originalOffset = _gridChild.LocalOffset;
   }

   // kinda bad but wcyd
   private void LateUpdate()
   {
      if (RefChild == null || RefChild.Size == _localSize) return;

      _gridChild.LocalOffset = _originalOffset - new Vector3(RefChild.Size, 0, 0);
      _localSize = RefChild.Size;
   }
}