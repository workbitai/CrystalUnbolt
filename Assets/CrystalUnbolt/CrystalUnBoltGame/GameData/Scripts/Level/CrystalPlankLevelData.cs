using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalPlankLevelData : IEquatable<CrystalPlankLevelData>
    {
        [SerializeField] CrystalPlankType plankType;
        public CrystalPlankType PlankType => plankType;

        [SerializeField] int plankLayer;
        public int PlankLayer => plankLayer;

        [SerializeField] Vector3 position;
        public Vector3 Position => position;
        [SerializeField] Vector3 rotation;
        public Vector3 Rotation => rotation;
        [SerializeField] Vector3 scale;
        public Vector3 Scale => scale;

        [SerializeField] List<Vector3> screwsPositions;
        public List<Vector3> ScrewsPositions => screwsPositions;

        public CrystalPlankLevelData()
        {
        }

        public CrystalPlankLevelData(CrystalPlankType CrystalPlankType, int plankLayer, Vector3 position, Vector3 rotation, Vector3 scale, List<Vector3> screwsPositions)
        {
            this.plankType = CrystalPlankType;
            this.plankLayer = plankLayer;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.screwsPositions = screwsPositions;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CrystalPlankLevelData);
        }

        public bool Equals(CrystalPlankLevelData other)
        {
            return other is not null &&
                   PlankType == other.PlankType &&
                   PlankLayer == other.PlankLayer &&
                   Position.Equals(other.Position) &&
                   Rotation.Equals(other.Rotation) &&
                   Scale.Equals(other.Scale) &&
                   screwsPositions.SequenceEqual(other.ScrewsPositions);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PlankType, PlankLayer, Position, Rotation, Scale, ScrewsPositions);
        }
    }
}