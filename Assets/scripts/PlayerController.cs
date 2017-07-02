﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour {
  public float speed;

  private FootController foot;
  public bool isAirborne;
  protected new Rigidbody2D rigidbody;
  protected PlayerController otherPlayer;
  public bool isBurden;
  private int otherMask;
  private bool isLocked;
  private float oomph;

  virtual public void Start() {
    rigidbody = GetComponent<Rigidbody2D>();
    foot = transform.Find("foot").GetComponent<FootController>();
    isAirborne = true;
    isLocked = false;
  }

  virtual public void Update() {
    if (isLocked) {
      return;
    }

    oomph = Input.GetAxis("Horizontal" + type);
    if (Input.GetButton("Jump" + type) && !isAirborne) {
      rigidbody.mass = 1;
      rigidbody.AddForce(Vector2.up * 300);
      isAirborne = true;
    }

    if (!isAirborne && Input.GetButtonDown("Transmit" + type)) {
      isLocked = true;
      StartCoroutine(TransmitAndUnlock());
    }

  }

  IEnumerator TransmitAndUnlock() {
    // Squat
    Vector2 startPosition = gameObject.transform.position;
    Vector2 endPosition = (Vector2) gameObject.transform.position - Vector2.up * 0.1f;
    Vector3 startScale = gameObject.transform.localScale;
    Vector3 endScale = new Vector3(1.2f, 0.8f, 1.0f);

    float startTime = Time.time;
    float targetTime = 0.1f;
    float elapsedTime = 0.0f;

    // Squat down and widen.
    while (elapsedTime < targetTime) {
      gameObject.transform.position = Vector2.Lerp(startPosition, endPosition, elapsedTime / targetTime);
      gameObject.transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / targetTime);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    // And return to form...
    startTime = Time.time;
    elapsedTime = 0.0f;
    while (elapsedTime < targetTime) {
      gameObject.transform.position = Vector2.Lerp(endPosition, startPosition, elapsedTime / targetTime);
      gameObject.transform.localScale = Vector3.Lerp(endScale, startScale, elapsedTime / targetTime);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    gameObject.transform.position = startPosition;
    gameObject.transform.localScale = startScale;

<<<<<<< HEAD
    if (IsConnectedToOther() && IsTransmittable()) {
      otherPlayer.isLocked = true;
=======
    if (isBurden && IsTransmittable()) {
>>>>>>> 140106797135aea20e4431d5f2ff320728981f62
      yield return StartCoroutine(Transmit());
    }

    isLocked = false;
    otherPlayer.isLocked = false;
  }

  void LateUpdate() {
    if (isBurden) {
      oomph += otherPlayer.oomph;
    }
    rigidbody.velocity = new Vector2(oomph * speed, rigidbody.velocity.y);
  }

  abstract public IEnumerator Transmit();
  abstract public bool IsTransmittable();
  abstract public void LevelEnd();
  abstract public void LevelStart();

  bool IsGrounded() {
    Collider2D hit = Physics2D.OverlapBox(foot.position, new Vector2(foot.width, foot.height), 0, Utilities.GROUND_MASK | otherMask);
    return hit != null;
  }

  public GameObject GetOnPointer() {
    Collider2D hit = Physics2D.OverlapBox(foot.position, new Vector2(foot.width, foot.height), 0, Utilities.GROUND_MASK);
    if (hit != null && hit.gameObject.tag == "pointer") {
      return hit.gameObject;
    }
    else if (hit != null && hit.gameObject.tag == "cell") {
      // does it have a parent that is a linked cell?
      Transform parentTransform = hit.gameObject.transform.parent;
      if (parentTransform != null && parentTransform.gameObject.tag == "linkedCell") {
        return parentTransform.GetComponentInChildren<PointerController>().gameObject;
      }
      else {
        return null;
      }
    }
    else {
      return null;
    }
  }

  public bool IsConnectedToOther() {

	  // check if close to other player and they are on a pointer
	  CircleCollider2D otherCollider = otherPlayer.GetComponent<CircleCollider2D>();
	  ColliderDistance2D distance = rigidbody.Distance(otherCollider);

	  return distance.distance < otherCollider.radius;
  }

  void OnCollisionEnter2D(Collision2D collision) {
    // If the player's foot touches the ground, we want to be able to jump
    // again.
    isAirborne = !IsGrounded();

    // If we land on top of the other people, let's reduce our mass to 0 so we
    // don't impede that player's jump.
    if ((1 << collision.gameObject.layer) == otherMask) {
      isBurden = transform.position.y > collision.gameObject.transform.position.y + 0.2;
      if (isBurden) {
        rigidbody.mass = 0;
      }
    }
  }

  void OnCollisionExit2D(Collision2D collision) {
    if (collision.gameObject == otherPlayer.gameObject) {
      rigidbody.mass = 1;
      isBurden = false;
    }
  }

  private string type;
  public string Type {
    get {
      return type;
    }

    set {
      type = value;
      if (type == "D") {
        otherMask = Utilities.PLAYER_R_MASK;
      } else {
        otherMask = Utilities.PLAYER_D_MASK;
      }
    }
  }
}
