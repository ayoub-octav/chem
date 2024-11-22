using Newtonsoft.Json;
using System;
using UnityEngine;

namespace ChemistryVR.Model
{
    [Serializable]
    public class Atom
    {
        public string name;
        public string symbol;
        public int atomicNumber;
        public float atomicWeight;
        public Color color;
        public int[] electronConfiguration;
        public double electronegativity;

        [JsonIgnore]
        public int spawnCounter = 0;
        [JsonIgnore]
        public bool isSpawnlimit;

        [JsonIgnore] // Ignore this in JSON serialization if needed
        public Guid uniqueID;

        public Atom(string name, string symbol, int number, float weight, Color color, int[] electronConfiguration, double electronegativity, int spawnCounter = 0, bool isSpawnlimit = false)
        {
            this.name = name;
            this.symbol = symbol;
            this.atomicNumber = number;
            this.atomicWeight = weight;
            this.color = color;
            this.electronConfiguration = electronConfiguration;
            this.electronegativity = electronegativity;
            this.spawnCounter = spawnCounter;
            this.isSpawnlimit = isSpawnlimit;
            this.uniqueID = Guid.NewGuid(); // Generate a unique ID
        }

        public override bool Equals(object obj)
        {
            if (obj is Atom atom)
            {
                Debug.Log($"checking Guid: this.Guid = {uniqueID} | other atom Guid = {atom.uniqueID}");
                // Check equality based on the unique ID
                return uniqueID == atom.uniqueID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            // Use the unique ID for hashing
            return uniqueID.GetHashCode();
        }
    }

/*
       public override bool Equals(object obj)
        {
            if (obj is Atom atom)
            {
                return symbol == atom.symbol && atomicNumber == atom.atomicNumber;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return symbol.GetHashCode() ^ atomicNumber.GetHashCode();
        }
    }*/
}