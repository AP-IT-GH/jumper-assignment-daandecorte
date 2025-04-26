1. Inleiding
In dit labo leren we een mlagent over aankomende obstakels springen.

2. Environment Setup
 
![App Screenshot](/images/scene.png)

In de scene staat een training object met een vloer en de agent. Het obstakel wordt tijdens het runnen in de scene geplaatst en beweegt richting de agent. Het obstakel kan van twee kanten komen. Het training object is opgeslagen als prefab en 12 keer in  de scene gezet om het trainen van de agent sneller te maken.

![App Screenshot](/images/components.png)

De agent heeft het behaviour parameter component. De belangrijkste parameters zijn hier: Space size op 3, discrete branch met branch size 0 op 2.
De agent heeft ook een agent controller script, een decision requester component, box collider en rigidbody.


Hieronder de AgentController en ObstacleController scripts.

    public class AgentController : Agent
    {
        public ObstacleController obstacleController;
        GameObject obstacle;
        public float speedMultiplier = 0.5f;
        public float rotationMultiplier = 5f;
        private bool hitObstacle;
        float distanceToObstacle;
        //Elke begin van een episode wordt deze functie aangeroepen.
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
        //De agent verzamelt de observaties waarmee er rekening gehouden mee kan worden.
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
        //Wanneer de observaties binnen zijn moet de agent iets doen. Dat gebeurt in deze functie.
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
        //Heuristic wordt gebruikt om de scene zelf te testen.
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionOut = actionsOut.DiscreteActions;
            if (IsGrounded()) {
                discreteActionOut[0] = Input.GetKey(KeyCode.Space) ? 1: 0;
            }
        }
        //Deze functie zorgt ervoor dat het obstakel van 2 kanten kan komen.
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

    public class ObstacleController : MonoBehaviour
    {
        public float speed = 5f;
        int side;
        
        //random sneheid wordt gekozen en er wordt gekeken naar waar het obstakel moet bewegen.
        void Start()
        {
            speed = Random.Range(3, 8);
            if (Mathf.Approximately(transform.localPosition.x, 0))
            {
                side = 1;
            }
            else if (Mathf.Approximately(transform.localPosition.z, 0))
            {
                side = 2;
            }
        }
        //het obstakel laten bewegen
        void Update()
        {
            if (side==1)
            {
                transform.localPosition += new Vector3(0, 0, speed * Time.deltaTime);
            }
            else if (side==2)
            {
                transform.localPosition += new Vector3(speed * Time.deltaTime, 0, 0);
            }
        }
    }

3. Acties, Observaties en Beloningen

![App Screenshot](/images/tabel.png)

4. Training
Jumper.yaml

    behaviors:
    Jumper:
        trainer_type: ppo
        hyperparameters:
        batch_size: 512
        buffer_size: 10240
        learning_rate: 3.0e-4
        beta: 5.0e-4
        epsilon: 0.2
        lambd: 0.99
        num_epoch: 3
        learning_rate_schedule: linear
        beta_schedule: constant
        epsilon_schedule: linear
        network_settings:
        normalize: true
        hidden_units: 128
        num_layers: 2
        reward_signals:
        extrinsic:
            gamma: 0.99
            strength: 1.0
        max_steps: 1000000
        time_horizon: 64
        summary_freq: 10000

Tensorboard resultaten diagram

![App Screenshot](/images/tensorboard.png)

Na 60k stappen begint de agent vooruitgang te maken. Het plafond is een gemiddeld 0.8 reward per episode wat na ongeveer 600k stappen wordt behaald.
