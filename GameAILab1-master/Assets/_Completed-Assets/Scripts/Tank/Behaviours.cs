using UnityEngine;
using NPBehave;
using System.Collections.Generic;

namespace Complete
{
    /*
    Example behaviour trees for the Tank AI.  This is partial definition:
    the core AI code is defined in TankAI.cs.

    Use this file to specifiy your new behaviour tree.
     */
    public partial class TankAI : MonoBehaviour
    {
        private Root CreateBehaviourTree() {

            switch (m_Behaviour) {

                case 0:
                    return FunBehaviour(1f, 1f, 1f);

                case 1:
                    return DeadlyBehaviour();

                case 2:
                    return FrightenedBehaviour();

                case 3:
                    return UnpredictableBehaviour();

                case 4:
                    return SpinBehaviour(-0.05f, 1f);
                case 5:
                    return TrackBehaviour();

                
                default:
                    return new Root (new Action(()=> Turn(0.1f)));
            }
        }

        /* Actions */

        private Node StopTurning() {
            return new Action(() => Turn(0));
        }

        private Node RandomFire() {
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));
        }

        //Only moves around, that's all
        private Root MoveOnly(float move)
        {
            return new Root(new Sequence(
                new Action(() => Move(move)
                )));
        }

        //Only fires, that's all
        private Root FireOnly(float fire)
        {
            return new Root(new Sequence(
                new Action(() => Fire(fire)
                )));
        }

        //Randomly picks a number between -1 and 1 and moves accordingly
        private Node RandomMove()
        {
            return new Action(() => Move(UnityEngine.Random.Range(-1f, 1f)));
        }



        /* Example behaviour trees */

        // Constantly spin and fire on the spot 
        private Root SpinBehaviour(float turn, float shoot) {
            return new Root(new Sequence(
                        new Action(() => Turn(turn)),
                        new Action(() => Fire(shoot))
                    ));
        }

        // Turn to face your opponent and fire
        private Root TrackBehaviour()
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,
                            // Stop turning and fire
                            new Sequence(StopTurning(),
                                        new Wait(2f),
                                        RandomFire())),
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,
                            // Turn right toward target
                            new Action(() => Turn(1f))),
                            // Turn left toward target
                            new Action(() => Turn(-1f))
                    )
                )
            );
        }


        //(0.Fun) Constantly spin, move and fire on the spot 
        private Root FunBehaviour(float move, float turn, float shoot){
       
            return new Root(new Sequence(
                        new Action(() => Move(move)),
                        new Action(() => Turn(turn)),
                        new Action(() => Fire(shoot))
                ));
        }

        //(1.Deadly) Tracks the opponent, moves towards it and constantly shoots
        private Root DeadlyBehaviour()
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,
                            // Move towards opponent, turn and fire
                            new Sequence(new Action(() => Move(0.4f)),
                        new Action(() => Turn(1f)),
                        new Action(() => Fire(-1f)))),
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,
                            // Turn right toward target
                            new Action(() => Turn(1f))),
                            // Turn left toward target
                            new Action(() => Turn(-1f))
                    )
                )
            );

        }

        //(2.Fightened) Tracks the opponent, moves away from it, doesn't shoot
        private Root FrightenedBehaviour()
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,
                            // Move away from opponent
                            new Sequence(
                                new Action(() => Turn(1f)),
                                new Action(() => Move(-0.5f))
                        )),
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,
                            // Turn right toward target
                            new Action(() => Turn(1f))),
                            // Turn left toward target
                            new Action(() => Turn(-1f))
                    )
                )
            );

        }

        //Stops turning, waits for a few seconds, then randomly moves and shoots according to the numbers generated by RandomMove() and RandomFire() methods respectively. The behaviour is different each time.
        private Root UnpredictableBehaviour()
        {
            return new Root(
                new Service(0.2f, UpdatePerception,
                    new Selector(
                        new BlackboardCondition("targetOffCentre",
                                                Operator.IS_SMALLER_OR_EQUAL, 0.1f,
                                                Stops.IMMEDIATE_RESTART,
                            // Move away from opponent
                            new Sequence(
                                
                                StopTurning(),
                                new Wait(3f),
                                RandomMove(),
                                RandomFire()

                        )),
                        new BlackboardCondition("targetOnRight",
                                                Operator.IS_EQUAL, true,
                                                Stops.IMMEDIATE_RESTART,
                            // Turn right toward target
                            new Action(() => Turn(1f))),
                            // Turn left toward target
                            new Action(() => Turn(-1f))
                    )
                )
            );

        }

        private void UpdatePerception() {
            Vector3 targetPos = TargetTransform().position;
            Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
            Vector3 heading = localPos.normalized;
            blackboard["targetDistance"] = localPos.magnitude;
            blackboard["targetInFront"] = heading.z > 0;
            blackboard["targetOnRight"] = heading.x > 0;
            blackboard["targetOffCentre"] = Mathf.Abs(heading.x);
        }

    }
}