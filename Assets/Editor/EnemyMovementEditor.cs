using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(EnemyMovement))]
public class EnemyMovementEditor : Editor
{
    private Collider[] Colliders = new Collider[10];

    private void OnSceneGUI()
    {
        EnemyMovement movement = (EnemyMovement)target;
        if (movement == null || movement.Player == null)
        {
            return;
        }

        int Hits = Physics.OverlapSphereNonAlloc(movement.Agent.transform.position, movement.LineOfSightChecker.Collider.radius, Colliders, movement.HidableLayers);
        if (Hits > 0)
        {
            int HitReduction = 0;
            for (int i = 0; i < Hits; i++)
            {
                if (Vector3.Distance(Colliders[i].transform.position, movement.Player.position) < movement.MinPlayerDistance)
                {
                    Handles.color = Color.red;
                    Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), Colliders[i].transform.position, Quaternion.identity, 0.25f, EventType.Repaint);
                    Handles.Label(Colliders[i].transform.position, $"{i} too close to target");
                    Colliders[i] = null;
                    HitReduction++;
                }
                else if (Colliders[i].bounds.size.y < movement.MinObstacleHeight)
                {
                    Handles.color = Color.red;
                    Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), Colliders[i].transform.position, Quaternion.identity, 0.25f, EventType.Repaint);
                    Handles.Label(Colliders[i].transform.position, $"{i} too small");
                    Colliders[i] = null;
                    HitReduction++;
                }

            }
            Hits -= HitReduction;

            System.Array.Sort(Colliders, movement.ColliderArraySortComparer);

            bool FoundTarget = false;

            for (int i = 0; i < Hits; i++)
            {
                if (NavMesh.SamplePosition(Colliders[i].transform.position, out NavMeshHit hit, 2f, movement.Agent.areaMask))
                {
                    if (!NavMesh.FindClosestEdge(hit.position, out hit, movement.Agent.areaMask))
                    {
                        Handles.color = Color.red;
                        Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), hit.position, Quaternion.identity, 0.25f, EventType.Repaint);
                        Handles.Label(hit.position, $"{i} (hit1) no edge found");
                    }

                    if (Vector3.Dot(hit.normal, (movement.Player.position - hit.position).normalized) < movement.HideSensitivity)
                    {
                        Handles.color = FoundTarget ? Color.yellow : Color.green;
                        FoundTarget = true;
                        Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), hit.position, Quaternion.identity, 0.25f, EventType.Repaint);
                        Handles.Label(hit.position, $"{i} (hit1) dot: {Vector3.Dot(hit.normal, (movement.Player.position - hit.position).normalized)}");
                    }
                    else
                    {
                        Handles.color = Color.red;
                        Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), hit.position, Quaternion.identity, 0.25f, EventType.Repaint);
                        Handles.Label(hit.position, $"{i} (hit1) dot: {Vector3.Dot(hit.normal, (movement.Player.position - hit.position).normalized)}");

                        if (NavMesh.SamplePosition(Colliders[i].transform.position - (movement.Player.position - hit.position).normalized * 2, out NavMeshHit hit2, 2f, movement.Agent.areaMask))
                        {
                            if (!NavMesh.FindClosestEdge(hit2.position, out hit2, movement.Agent.areaMask))
                            {
                                Handles.color = Color.red;
                                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), hit2.position, Quaternion.identity, 0.25f, EventType.Repaint);
                                Handles.Label(hit.position, $"{i} (hit2) no edge found");
                            }

                            if (Vector3.Dot(hit2.normal, (movement.Player.position - hit2.position).normalized) < movement.HideSensitivity)
                            {
                                Handles.color = FoundTarget ? Color.yellow : Color.green;
                                FoundTarget = true;
                                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), hit2.position, Quaternion.identity, 0.25f, EventType.Repaint);
                                Handles.Label(hit2.position, $"{i} (hit2) dot: {Vector3.Dot(hit2.normal, (movement.Player.position - hit2.position).normalized)}");
                            }
                            else
                            {
                                Handles.color = Color.red;
                                Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), hit2.position, Quaternion.identity, 0.25f, EventType.Repaint);
                                Handles.Label(hit2.position, $"{i} (hit2) dot: {Vector3.Dot(hit2.normal, (movement.Player.position - hit2.position).normalized)}");
                            }
                        }
                        else
                        {
                            Handles.color = Color.red;
                            Handles.SphereHandleCap(GUIUtility.GetControlID(FocusType.Passive), hit2.position, Quaternion.identity, 0.25f, EventType.Repaint);
                            Handles.Label(hit.position, $"{i} Hit 2 could not sampleposition");
                        }
                    }
                }
            }
        }
    }
}
