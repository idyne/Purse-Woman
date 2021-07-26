using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;

public class Character : MonoBehaviour
{
    [SerializeField] private float speed = 1;
    [SerializeField] private float angle = 45;
    [SerializeField] private float obstacleCooldown = 1f;
    [SerializeField] private LayerMask raycastLayerMask = 0;
    [SerializeField] private LayerMask bonusPartLayerMask = 0;
    [SerializeField] private Transform leftArm, leftForeArm, leftBagsTransform, rightArm, rightForeArm, rightBagsTransform;
    private Swerve1D swerve = null;
    private Rigidbody rb = null;
    private Vector3 anchor = Vector3.zero;
    private State state = State.IDLE;
    private Animator anim = null;
    private float previousObstacleTime = 0;
    [SerializeField] private List<GameObject> leftBags = null;
    [SerializeField] private List<GameObject> rightBags = null;
    private int numberOfBagsOnLeft = 0;
    private int numberOfBagsOnRight = 0;
    private Rail currentRail = null;
    [SerializeField] public bool OnBonus = false;
    private float leftArmLength = 0.85f;
    private float rightArmLength = 0.85f;
    private BonusPart currentBonusPart = null;
    private bool railLock = true;
    [SerializeField] private int numberOfBags = 0;

    private int NumberOfBags
    {
        get => numberOfBags;
        set
        {
            numberOfBags = value;
            anim.SetInteger("BAGS", value);
        }
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        swerve = InputManager.CreateSwerve1D(Vector2.right, Screen.width / 2);
        swerve.OnStart = () =>
        {
            anchor = transform.position;
        };
    }

    private void Start()
    {
        DeactivateRagdoll();
    }

    private void Update()
    {
        if (GameManager.Instance.State == GameManager.GameState.STARTED)
        {
            SendRayDown();
            if (state == State.BALANCE)
                Tilt();
            MoveForward();
            CheckInput();
            if (OnBonus)
            {
                SendBonusPartRay();
                if (transform.position.y - 1f <= currentBonusPart.transform.position.y)
                {
                    for (int i = 2; i < 6; i++)
                    {
                        ObjectPooler.Instance.SpawnFromPool("Confetti", currentBonusPart.transform.position + Vector3.right * i, Quaternion.Euler(-90, 0, 0));
                        ObjectPooler.Instance.SpawnFromPool("Confetti", currentBonusPart.transform.position - Vector3.right * i, Quaternion.Euler(-90, 0, 0));
                    }
                    MainLevelManager._Instance.GemMultiplier = currentBonusPart.Multiplier;
                    MainLevelManager._Instance.FinishLevel(true);
                }
            }
        }
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.H))
            RemoveBag();
        if (Input.GetKeyDown(KeyCode.J))
            AddBag();
        if (swerve.Active)
        {
            if (state == State.WALKING || state == State.RAIL)
                MoveHorizontally(swerve.Rate);
            else if (state == State.BALANCE)
                Balance(swerve.Rate);
        }
        if (state == State.RAIL)
        {
            if (!railLock && (transform.position.x + rightArmLength + 0.1f < currentRail.RightX || transform.position.x - leftArmLength - 0.1f > currentRail.LeftX || transform.position.x > currentRail.RightX || transform.position.x < currentRail.LeftX))
            {
                Fall();
            }
        }
    }

    private void SendRayDown()
    {
        if (Physics.Raycast(transform.position - Vector3.forward * 0.4f, Vector3.down, out RaycastHit hit, 50, raycastLayerMask))
        {
            if (state == State.WALKING)
            {
                if (hit.transform.CompareTag("Balance"))
                {
                    swerve.Reset();
                    swerve.OnStart();
                    ChangeState(State.BALANCE);
                }
                else if (hit.transform.CompareTag("Rail"))
                {
                    currentRail = hit.transform.GetComponent<Rail>();
                    railLock = true;
                    transform
                        .LeanMoveY(currentRail.transform.position.y, Mathf.Sqrt((transform.position.y - currentRail.transform.position.y) / 5))
                        .setEaseInQuad()
                        .setOnComplete(() =>
                        {
                            railLock = false;
                        });
                    ChangeState(State.RAIL);
                }

            }
            else if (state == State.BALANCE && hit.transform.CompareTag("Road"))
            {
                swerve.Reset();
                swerve.OnStart();
                transform.LeanRotateZ(0, 0.3f).setEaseInQuad();
                ChangeState(State.WALKING);
            }
            else if (state == State.RAIL)
            {
                if (hit.transform.CompareTag("Road"))
                {
                    transform.LeanMoveY(hit.transform.position.y, Mathf.Sqrt((transform.position.y - hit.transform.position.y) / 5)).setEaseInQuad();

                    ChangeState(State.WALKING);
                }
                else if (hit.transform.CompareTag("Rail") && hit.transform != currentRail.transform)
                {
                    currentRail = hit.transform.GetComponent<Rail>();
                    railLock = true;
                    transform
                        .LeanMoveY(currentRail.transform.position.y, Mathf.Sqrt((transform.position.y - currentRail.transform.position.y) / 5))
                        .setEaseInQuad()
                        .setOnComplete(() =>
                        {
                            railLock = false;
                        });
                }
                /*else if(hit.transform.CompareTag("Bonus Part"))
                {
                    MainLevelManager._Instance.GemMultiplier = hit.transform.GetComponent<BonusPart>().Multiplier;
                    MainLevelManager._Instance.FinishLevel(true);
                }*/
            }
        }
        else
        {
            Fall();
        }
    }

    public bool ChangeState(State newState)
    {
        switch (newState)
        {
            case State.FALLING:
                if (state == State.RAIL)
                {
                    state = newState;
                }
                break;
            case State.WALKING:
                if (state == State.BALANCE || state == State.IDLE || state == State.RAIL)
                {
                    state = newState;
                    anim.SetTrigger("WALKING");
                }
                break;
            case State.RAIL:
                if (state == State.WALKING)
                {
                    state = newState;
                    anim.SetTrigger("RAIL");
                }
                break;
            case State.BALANCE:
                if (state == State.WALKING)
                {
                    state = newState;
                }
                break;
            case State.FAIL:
                if (state == State.WALKING || state == State.BALANCE)
                {
                    state = newState;
                    anim.SetTrigger("FAIL");
                }
                break;
            case State.SUCCESS:
                if (state == State.WALKING)
                {
                    state = newState;
                    anim.SetTrigger("SUCCESS");
                }
                break;
            default:
                return false;
        }
        return true;
    }

    private void ActivateRagdoll()
    {
        CameraFollow.Instance.PhysicsFollow = true;
        anim.enabled = false;
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = false;
        }
    }
    private void DeactivateRagdoll()
    {
        CameraFollow.Instance.PhysicsFollow = false;
        anim.enabled = true;
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = true;
        }
    }

    private void Tilt()
    {
        Vector3 rot = transform.rotation.eulerAngles;
        rot = Vector3.MoveTowards(rot, rot + ((rot.z < 180) ? 1 : -1) * Vector3.forward, Time.deltaTime * angle / 2f);
        transform.rotation = Quaternion.Euler(rot);
        if ((rot.z < 180 && rot.z > angle) || (rot.z > 180 && rot.z < 360 - angle))
        {
            Fall();
        }
    }
    private void Fall()
    {
        ActivateRagdoll();
        if (!OnBonus)
            CameraFollow.Instance.SwitchFallCamera();
        if (!OnBonus)
        {
            ChangeState(State.FAIL);
            MainLevelManager._Instance.FinishLevel(false);
        }
        else
        {
            ChangeState(State.FALLING);
        }

    }

    private void Balance(float rate)
    {
        if (Mathf.Abs(rate) == 0) return;
        Vector3 desiredRot = transform.rotation.eulerAngles;
        desiredRot = Vector3.MoveTowards(desiredRot, desiredRot + Vector3.forward * rate * -angle * 1.5f, Time.deltaTime * angle * 1);
        transform.rotation = Quaternion.Euler(desiredRot);
    }

    private void MoveForward()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + Vector3.forward, Time.deltaTime * (state == State.WALKING ? speed : state == State.RAIL ? speed * 1.3f : (speed / 2f)));
    }

    private void MoveHorizontally(float rate)
    {
        if (Mathf.Abs(rate) == 0) return;
        Vector3 desiredPos = transform.position;
        float clampMin = state == State.RAIL ? currentRail.LeftX + 0.25f : -2.25f;
        float clampMax = state == State.RAIL ? currentRail.RightX - 0.25f : 2.25f;
        desiredPos.x = (anchor + Vector3.right * rate * (4.5f)).x;
        desiredPos.x = Mathf.Clamp(desiredPos.x, clampMin, clampMax);
        if (transform.position.x == clampMin || transform.position.x == clampMax)
        {
            swerve.Reset();
            swerve.OnStart();
        }
        transform.position = desiredPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.State != GameManager.GameState.STARTED)
            return;
        if (other.CompareTag("Collectible"))
        {
            ICollectible collectible = other.GetComponent<ICollectible>();
            collectible.GetCollected();
        }
        else if (other.CompareTag("Obstacle"))
        {
            Stumble();
        }
    }

    public void AddBag()
    {
        if (NumberOfBags++ < 16)
        {
            if (numberOfBagsOnRight < numberOfBagsOnLeft)
            {
                int index = numberOfBagsOnRight++;
                if (index > 0)
                {
                    rightArmLength += 0.235f;
                    float desiredLength = rightArm.localScale.y + 0.8f;
                    rightArm.LeanScaleY(desiredLength, 0.3f);
                    rightForeArm.LeanScaleY(1f / desiredLength, 0.3f);
                    rightBagsTransform.LeanScaleY((0.01f / ((index + 1) * 0.8f)), 0.3f);
                    LeanTween.delayedCall(0.3f, () =>
                    {
                        rightBags[index].SetActive(true);
                        rightBags[index].transform.localScale = Vector3.zero;
                        rightBags[index].LeanScale(Vector3.one, 0.3f).setEaseOutElastic();
                    });
                }
                else
                {
                    rightBags[index].SetActive(true);
                    rightBags[index].transform.localScale = Vector3.zero;
                    rightBags[index].LeanScale(Vector3.one, 0.3f).setEaseOutElastic();
                }

            }
            else
            {
                int index = numberOfBagsOnLeft++;
                if (index > 0)
                {
                    leftArmLength += 0.235f;
                    float desiredLength = leftArm.localScale.y + 0.8f;
                    leftArm.LeanScaleY(desiredLength, 0.3f);
                    leftForeArm.LeanScaleY(1f / desiredLength, 0.3f);
                    leftBagsTransform.LeanScaleY((0.01f / ((index + 1) * 0.8f)), 0.3f);
                    LeanTween.delayedCall(0.3f, () =>
                    {
                        leftBags[index].SetActive(true);
                        leftBags[index].transform.localScale = Vector3.zero;
                        leftBags[index].LeanScale(Vector3.one, 0.3f).setEaseOutElastic();
                    });
                }
                else
                {
                    leftBags[index].SetActive(true);
                    leftBags[index].transform.localScale = Vector3.zero;
                    leftBags[index].LeanScale(Vector3.one, 0.3f).setEaseOutElastic();
                }
            }
        }

    }

    public void RemoveBag()
    {
        if (NumberOfBags <= 0)
        {
            MainLevelManager._Instance.FinishLevel(false);
            return;
        }
        else if (NumberOfBags-- <= 16)
        {
            if (numberOfBagsOnRight < numberOfBagsOnLeft)
                RemoveBagFromLeft();
            else
                RemoveBagFromRight();
        }
    }

    public void RemoveBagFromLeft()
    {
        int index = --numberOfBagsOnLeft;
        if (index > 0)
        {
            leftArmLength -= 0.235f;
            float desiredLength = leftArm.localScale.y - 0.8f;
            leftArm.LeanScaleY(desiredLength, 0.3f);
            leftForeArm.LeanScaleY(1f / desiredLength, 0.3f);
            leftBagsTransform.LeanScaleY((0.01f / ((index) * 0.8f)), 0.3f);
            LeanTween.delayedCall(0.3f, () =>
            {
                leftBags[index].SetActive(false);
                leftBags[index].transform.localScale = Vector3.one;
                leftBags[index].LeanScale(Vector3.zero, 0.3f).setEaseOutElastic();
            });
        }
        else
        {
            leftBags[index].SetActive(false);
            leftBags[index].transform.localScale = Vector3.one;
            leftBags[index].LeanScale(Vector3.zero, 0.3f).setEaseOutElastic();
        }
    }

    public void RemoveBagFromRight()
    {
        int index = --numberOfBagsOnRight;
        if (index > 0)
        {
            rightArmLength -= 0.235f;
            float desiredLength = rightArm.localScale.y - 0.8f;
            rightArm.LeanScaleY(desiredLength, 0.3f);
            rightForeArm.LeanScaleY(1f / desiredLength, 0.3f);
            rightBagsTransform.LeanScaleY((0.01f / ((index) * 0.8f)), 0.3f);
            LeanTween.delayedCall(0.3f, () =>
            {
                rightBags[index].SetActive(false);
                rightBags[index].transform.localScale = Vector3.one;
                rightBags[index].LeanScale(Vector3.zero, 0.3f).setEaseOutElastic();
            });
        }
        else
        {
            rightBags[index].SetActive(false);
            rightBags[index].transform.localScale = Vector3.one;
            rightBags[index].LeanScale(Vector3.zero, 0.3f).setEaseOutElastic();
        }
    }

    private void Stumble()
    {
        if (Time.time <= previousObstacleTime + obstacleCooldown)
            return;
        anim.SetTrigger("STUMBLE");
        previousObstacleTime = Time.time;
        RemoveBag();
    }

    public void HitByThief()
    {
        anim.SetTrigger("STUMBLE");
        ObjectPooler.Instance.SpawnFromPool("Hit Woman Effect", transform.position + Vector3.up * 1, Quaternion.identity);
        for (int i = 0; i < leftBags.Count; i++)
            leftBags[i].SetActive(false);
        numberOfBagsOnLeft = 0;
        for (int i = 0; i < rightBags.Count; i++)
            rightBags[i].SetActive(false);
        numberOfBagsOnRight = 0;
        NumberOfBags = 0;
        rightArm.LeanScaleY(1, 0.3f);
        rightForeArm.LeanScaleY(1f, 0.3f);
        rightBagsTransform.LeanScaleY(0.01f, 0.3f);
        leftArm.LeanScaleY(1, 0.3f);
        leftForeArm.LeanScaleY(1f, 0.3f);
        leftBagsTransform.LeanScaleY(0.01f, 0.3f);
        leftArmLength = 0.85f;
        rightArmLength = 0.85f;
    }

    public void HitByThief(int index, bool isRight, Vector3 position)
    {
        anim.SetTrigger("STUMBLE");
        ObjectPooler.Instance.SpawnFromPool("Hit Bag Effect", position, Quaternion.identity);
        if (isRight)
        {
            for (int i = index; i < numberOfBagsOnRight; i++)
                rightBags[i].SetActive(false);
            int number = numberOfBagsOnRight - index;
            NumberOfBags -= number;
            rightArmLength = Mathf.Clamp(rightArmLength - 0.235f * (number), 0.85f, 100);
            numberOfBagsOnRight -= number;
            float desiredLength = 1 + Mathf.Clamp(numberOfBagsOnRight - 1, 0, numberOfBagsOnRight) * 0.8f;
            rightArm.LeanScaleY(desiredLength, 0.3f);
            rightForeArm.LeanScaleY(1f / desiredLength, 0.3f);
            rightBagsTransform.LeanScaleY(0.01f / desiredLength, 0.3f);
        }
        else
        {
            for (int i = index; i < numberOfBagsOnLeft; i++)
                leftBags[i].SetActive(false);
            int number = numberOfBagsOnLeft - index;
            NumberOfBags -= number;
            leftArmLength = Mathf.Clamp(leftArmLength - 0.235f * (number), 0.85f, 100);
            numberOfBagsOnLeft -= number;
            float desiredLength = 1 + Mathf.Clamp(numberOfBagsOnLeft - 1, 0, numberOfBagsOnLeft) * 0.8f;
            leftArm.LeanScaleY(desiredLength, 0.3f);
            leftForeArm.LeanScaleY(1f / desiredLength, 0.3f);
            leftBagsTransform.LeanScaleY(0.01f / desiredLength, 0.3f);
        }
    }

    private void SendBonusPartRay()
    {
        if (Physics.Raycast(transform.position - Vector3.forward * 0.4f, Vector3.down, out RaycastHit hit, 100, bonusPartLayerMask))
        {
            if (hit.transform.CompareTag("Bonus Part"))
                currentBonusPart = hit.transform.GetComponent<BonusPart>();
        }
    }

    public enum State { IDLE, WALKING, BALANCE, RAIL, SUCCESS, FAIL, FALLING }
}
