using System;
using System.Collections.Generic;
using UnityEngine;

namespace RetwineMake.Gameplay
{
    public enum ObjectiveState { Pending, Complete, Failed }

    [Serializable]
    public class Objective
    {
        public string id;
        public string description;
        public bool optional;

        [NonSerialized] public ObjectiveState state = ObjectiveState.Pending;
    }

    public class ObjectiveManager : MonoBehaviour
    {
        public static ObjectiveManager Instance { get; private set; }

        [SerializeField] List<Objective> objectives = new List<Objective>();

        public event Action<Objective> OnObjectiveUpdated;
        public event Action OnMissionComplete;
        public event Action OnMissionFailed;

        bool missionEnded;

        public IReadOnlyList<Objective> Objectives => objectives;

        void Awake()
        {
            Instance = this;
        }

        public void CompleteObjective(string id)
        {
            var o = objectives.Find(x => x.id == id);
            if (o == null || o.state != ObjectiveState.Pending)
                return;

            o.state = ObjectiveState.Complete;
            Debug.Log($"[Objective] Complete: {o.description}");
            OnObjectiveUpdated?.Invoke(o);
        }

        public void FailObjective(string id)
        {
            var o = objectives.Find(x => x.id == id);
            if (o == null || o.state != ObjectiveState.Pending)
                return;

            o.state = ObjectiveState.Failed;
            Debug.Log($"[Objective] Failed: {o.description}");
            OnObjectiveUpdated?.Invoke(o);
        }

        public bool AllMandatoryComplete()
        {
            foreach (var o in objectives)
                if (!o.optional && o.state != ObjectiveState.Complete)
                    return false;
            return true;
        }

        public void CompleteMission()
        {
            if (missionEnded || !AllMandatoryComplete())
                return;

            missionEnded = true;
            Debug.Log("[Objective] MISSION COMPLETE");
            OnMissionComplete?.Invoke();
        }

        public void FailMission()
        {
            if (missionEnded)
                return;

            missionEnded = true;
            Debug.Log("[Objective] MISSION FAILED");
            OnMissionFailed?.Invoke();
        }
    }
}
