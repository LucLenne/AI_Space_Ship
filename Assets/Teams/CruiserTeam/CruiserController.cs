using BehaviorDesigner.Runtime;
using DoNotModify;
using System.Collections.Generic;
using UnityEngine;

namespace CruiserTeam
{
    public class CruiserController : BaseSpaceShipController
    {
        [SerializeField] private BehaviorTree tree;
        [SerializeField] private Clustering clusteringTool;

        [Space(10)]
        [SerializeField] private List<WayPointCluster> _clusters;
        
        [Space(10)]
        [SerializeField] private float _whiskersLenght = 1f;
        [SerializeField] private List<float> _whiskersRadius = new List<float>();
        private List<Vector2> _whiskersVectors = new List<Vector2>();

        private bool _passInit = false;
        private GameData _gameData;
        private SpaceShipView _spaceShipView;
        public static CruiserController Instance;

        public InputData inputData;

        public SpaceShipView SpaceShipView { get => _spaceShipView; private set => _spaceShipView = value; }
        public GameData GameData { get => _gameData; private set => _gameData = value; }
        public List<WayPointCluster> Clusters { get => _clusters; set => _clusters = value; }

        public int GetEnemyOwner
        {
            get
            {
                if (SpaceShipView.Owner == 1)
                    return 0;
                else
                    return 1;
            }
        }
        public SpaceShipView GetEnemySpaceship
        {
            get => GameData.SpaceShips[GetEnemyOwner];
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }


        public override void Initialize(SpaceShipView spaceship, GameData data)
        {
            // Verification before Initialization
            #region Verification
            if (tree == null || !TryGetComponent<BehaviorTree>(out tree))
            {
                Debug.LogError($"No BehaviorTree Component found", gameObject);
                return;
            }
            if (clusteringTool == null || !TryGetComponent<Clustering>(out clusteringTool))
            {
                Debug.LogError($"No Clustering Component found", gameObject);
                return;
            }

            _passInit = true;
            #endregion

            _clusters = clusteringTool.InitializeClustering(data);
            
            _whiskersRadius.Sort();
            _whiskersRadius.Reverse();
        }

        public override InputData UpdateInput(SpaceShipView spaceship, GameData data)
        {
            GameData = data;
            SpaceShipView = spaceship;
            SpaceShipView otherSpaceship = data.GetSpaceShipForOwner(1 - spaceship.Owner);
            
            Whisky(ref inputData.targetOrientation);

            if (Vector2.Distance(SpaceShipView.Position, otherSpaceship.Position) <= 2f &&
                !SpaceShipView.HasFiredShockwave)
                inputData.fireShockwave = true;
            
            inputData.shoot = AimingHelpers.CanHit(spaceship,  CruiserController.Instance.GetEnemySpaceship.Position, CruiserController.Instance.GetEnemySpaceship.Velocity, 5) &&
                                                         CruiserController.Instance.SpaceShipView.Energy >= 0.6f &&
                                                         CruiserController.Instance.GetEnemySpaceship.HitPenaltyCountdown == 0 &&
                                                         CruiserController.Instance.GetEnemySpaceship.StunPenaltyCountdown == 0;
            
            if (inputData.dropMine && spaceship.HasDroppedMine)
                inputData.dropMine = false;
            
            return inputData;
        }
        
        private void Whisky(ref float targetOrientation)
        {
            float Fradians = SpaceShipView.Orientation * Mathf.Deg2Rad;
            float Fsin = Mathf.Sin(Fradians);
            float Fcos = Mathf.Cos(Fradians);
            
            Vector2 forward = new Vector2(Fcos * 1 - Fsin * 1, Fsin * 1 + Fcos * 1).normalized;

            foreach (AsteroidView current in GameData.Asteroids)
            {
                if (Vector2.Distance(SpaceShipView.Position + (forward * _whiskersLenght), current.Position) <=
                    current.Radius + _whiskersLenght)
                {
                    Fradians = (SpaceShipView.Orientation + 90) * Mathf.Deg2Rad;
                    Fsin = Mathf.Sin(Fradians);
                    Fcos = Mathf.Cos(Fradians);
                    Vector2 left = new Vector2(Fcos * 1 - Fsin * 1, Fsin * 1 + Fcos * 1) + SpaceShipView.Position;
                    Fradians = (SpaceShipView.Orientation - 90) * Mathf.Deg2Rad;
                    Fsin = Mathf.Sin(Fradians);
                    Fcos = Mathf.Cos(Fradians);
                    Vector2 right = new Vector2(Fcos * 1 - Fsin * 1, Fsin * 1 + Fcos * 1) + SpaceShipView.Position;

                    //Debug.Log($"{Vector2.Distance(right, current.Position)} | {Vector2.Distance(left, current.Position)}");
                    if (Vector2.Distance(right, current.Position) < Vector2.Distance(left, current.Position))
                    {
                        targetOrientation -= 90f;
                        //Debug.Log("turn left");
                    }
                    else
                    {
                        targetOrientation += 90f;
                        //Debug.Log("turn right");
                    }

                    inputData.shoot = false;
                    return;
                }
            }
        }
            
        /*Vector2 Target(GameData data, SpaceShipView spaceship)
        {
            int index = 0;
            float closerWayPoint = Mathf.Infinity;
            for (int i = 0; i < _clusters.Count; i++)
            {
                float actualDistance = Vector2.Distance(spaceship.Position, _clusters[i].averagePos);
                if (actualDistance < closerWayPoint && _clusters[i].nbCapturablePoints(spaceship.Owner) > 0)
                {
                    closerWayPoint = actualDistance;
                    index = i;
                }
            }

            return _clusters[index].OptimalTrajectory(spaceship)[0];
        }*/
    }
}
