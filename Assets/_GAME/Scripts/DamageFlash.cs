using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
   [SerializeField] private SpriteRenderer renderer;

   [SerializeField] private float duration;

   private static readonly int flashAmount =
        Shader.PropertyToID("_FlashAmount");


   private Material defaultMat; 

   private Coroutine flashCoroutine;

   public void OnInit()
   {
      defaultMat = renderer.material;
   }

   public void OnDespawn()
   {
      StopAllCoroutines();
   }

   public void Flash()
   {
      if(flashCoroutine != null)
      {
         StopCoroutine(flashCoroutine);
      }

      flashCoroutine = StartCoroutine(IEFlash());
   }

   private IEnumerator IEFlash()
   {
      
      defaultMat.SetFloat(flashAmount, 1f);

      yield return new WaitForSeconds(duration);

      
      defaultMat.SetFloat(flashAmount, 0f);

      flashCoroutine = null;
   }
}
