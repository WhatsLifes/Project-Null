using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
   [SerializeField] private Sanity sanity;
   public KeyCode useKey = KeyCode.Q;
   // bool for holding syringe
   [Header("Things in Inventory")]
   public bool holdingSyringe;
   public int pictureCount;
   [Header("Audio")]
   public AudioSource audioSource;
   public AudioClip injectSyringe;
   

   void Start()
   {
      holdingSyringe = false;
   }

   void Update()
   {
      if(Input.GetKeyDown(useKey))
         useSyringe();
   }
   
   public void pickUpSyringe()
   {
      holdingSyringe = true;
   }

   public void pickUpPicture()
   {
      pictureCount++;
   }
   
   void useSyringe()
   {
      if (!holdingSyringe) return;
      audioSource.PlayOneShot(injectSyringe);
      Debug.Log("using syringe");
      sanity.RestoreSanity(100f);
      holdingSyringe = false;
   }
}
