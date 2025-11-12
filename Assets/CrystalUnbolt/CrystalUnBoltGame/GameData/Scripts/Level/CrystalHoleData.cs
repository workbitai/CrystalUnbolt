using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalHoleData : IEquatable<CrystalHoleData>
    {
        [SerializeField] Vector3 position;
        public Vector3 Position => position;

        [SerializeField] bool hasScrew;
        public bool HasScrew => hasScrew;

        public CrystalHoleData()
        {
        }

        public CrystalHoleData(Vector3 position, bool hasScrew)
        {
            this.position = position;
            this.hasScrew = hasScrew;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CrystalHoleData);
        }

        public bool Equals(CrystalHoleData other)
        {
            return other is not null &&
                   Position.Equals(other.Position) &&
                   HasScrew == other.HasScrew;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, HasScrew);
        }
    }
}
