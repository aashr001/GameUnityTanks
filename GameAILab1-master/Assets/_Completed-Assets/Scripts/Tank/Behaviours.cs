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

                case 1:
                    return SpinBehaviour(-0.05f, 1f);
                case 2:
                    return TrackBehaviour();

                case 3:
                    return TestTrackBehaviour();

                case 4:
                    return TestSpinBehaviour(0.9f, -0.5f);

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
                        new BlackboardCondition("targetDistance",
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


        //Just testing what the program does
        private Root TestTrackBehaviour()
        {
            return new Root(
               new Service(0.2f, UpdatePerception,
                   new Selector(
                       
                            new BlackboardCondition("targetDistance",
                                               Operator.IS_EQUAL, true,
                                               Stops.IMMEDIATE_RESTART,
                           // Stop turning and fire
                           new Sequence(new Action(() => Turn(0.2f)),
                                       new Action(() => Turn(-0.2f)),
                                       RandomFire())),
                       new BlackboardCondition("targetOnRight",
                                               Operator.IS_EQUAL, true,
                                               Stops.IMMEDIATE_RESTART,
                           // Turn right toward target
                           new Action(() => Turn(0.2f))),
                           // Turn left toward target
                           new Action(() => Turn(-0.2f))
                   )
               )
           );
        }


        //Lalallaaa
        //Testing the spin behaviour
        private Root TestSpinBehaviour(float right, float left)
        {
           
            return new Root(new Sequence(
                        new Action(() => Turn(right)),
                        StopTurning(),
                        new Wait(0.1f),
                        new Action(()=> Turn(left))));
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