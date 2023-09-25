using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

 public interface CriticalNodeFactory
 {
    public CriticalNode CreateCriticalNode(List<Citizen> citizens);
}