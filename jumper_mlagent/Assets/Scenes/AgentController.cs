using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;

public class AgentController : Agent
{
    public ObstacleController obstacleController;
    GameObject obstacle;
    public float speedMultiplier = 0.5f;
    public float rotationMultiplier = 5f;
    private bool hitObstacle;
    float distanceToObstacle;
    public override void OnEpisodeBegin()
    {
        obstacle = Instantiate(obstacleController.gameObject, transform.parent);
        GetRandomObstaclePosition();

        if (this.transform.localPosition.y < 0 || this.transform.localPosition.y > 100)
        {
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
            this.transform.localRotation = Quaternion.Euler(0, 0, 0);
            gameObject.GetComponent<Rigidbody>();
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        distanceToObstacle = Vector3.Distance(this.transform.localPosition, obstacle.transform.localPosition);
        
        sensor.AddObservation(this.transform.localPosition.y);
        sensor.AddObservation(distanceToObstacle);
        sensor.AddObservation(obstacleController.speed);
    }
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int jumpAction = actionBuffers.DiscreteActions[0];
        if (IsGrounded() && jumpAction == 1)
        {
            gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }
        
        if (distanceToObstacle < 1.4f)
        {
            Destroy(obstacle);
            EndEpisode();
        }
        if (obstacle.transform.localPosition.x > 10 || obstacle.transform.localPosition.z > 10)
        {
            Destroy(obstacle);
            SetReward(1f);
            EndEpisode();
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionOut = actionsOut.DiscreteActions;
        if (IsGrounded()) {
            discreteActionOut[0] = Input.GetKey(KeyCode.Space) ? 1: 0;
        }
    }
    void GetRandomObstaclePosition()
    {
        switch (Random.Range(1, 3))
        {
            case 1:
                obstacle.transform.localPosition = new Vector3(-10f, 0.5f, 0);
                obstacle.transform.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case 2:
                obstacle.transform.localPosition = new Vector3(0, 0.5f, -10f);
                obstacle.transform.localRotation = Quaternion.Euler(0, 90, 0);
                break;
            default:
                break;
        }
    }
}
