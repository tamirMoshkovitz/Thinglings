using System;
using _SLIME.BaseScripts;
using UnityEngine;


namespace _SLIME.LittleBoss
{
    [Serializable]
    public struct LittleBossSpellAttackSettings
    {
            
    }
    
    
    [Serializable]
    public struct LittleBossSpellAttackRef
    {
        public Transform littleBossTransform;
        public Transform[] targets;
    }
    
    public class LittleBossSpellAttackLogic: LittleBossBaseLogic
    {
        
        public LittleBossSpellAttackSettings Set;
        private readonly LittleBossSpellAttackRef _ref;
        private readonly ProjectMonoBehavior _mono;

        public LittleBossSpellAttackLogic(LittleBossSpellAttackSettings set, 
            LittleBossSpellAttackRef reference, ProjectMonoBehavior mono)
        {
             Set = set;
            _ref = reference;
            _mono = mono;
        }


        public void StartLogic()
        {
           Debug.Log("Started Spell Attack logic");
        }

        public void EndLogic()
        {
            
        }
    }
}