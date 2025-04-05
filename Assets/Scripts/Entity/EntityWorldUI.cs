using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EntityWorldUI : MonoBehaviour
{
    public TMP_Text HealthText;
    public Slider HealthSlider;
    public Slider HealthSliderAnim;
    public Slider ArmorSlider;
    public Slider ArmorSliderAnim;
    public Image targetImage;
    public Image reloadImage;
    public float duration = 1.0f;
    private Coroutine _lerpCoroutine = null;
    public PlayerController PlayerController;

    void Start()
    {
        HealthSliderAnim.maxValue = PlayerController.Armor;
        ArmorSlider.maxValue = PlayerController.Armor;
        HealthSliderAnim.maxValue = PlayerController.Health;
        HealthSlider.maxValue = PlayerController.Health;
    }

    void Update()
    {
        float currentHealth = PlayerController.Health;
        float currentArmor = PlayerController.Armor;
        ArmorSlider.value = currentArmor;
        HealthSlider.value = currentHealth;
        HealthText.text = HealthSlider.value.ToString();

        reloadImage.fillAmount = Mathf.Clamp01(PlayerController.ReloadDelay / PlayerController.Gun.reloadDelay) ;
        if (_lerpCoroutine != null)
        {
            StopCoroutine(_lerpCoroutine);
        }

        _lerpCoroutine = StartCoroutine(LerpStretch(currentHealth, currentArmor));

        GameObject _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        transform.rotation = _mainCamera.transform.rotation;
    }

    private IEnumerator LerpStretch(float targetHealth, float targetArmor)
    {
        float elapsedTime = 0f;
        float startValueHealth = HealthSliderAnim.value;
        float startValueArmor = ArmorSliderAnim.value;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float ht = elapsedTime / duration;
            ArmorSliderAnim.value = Mathf.Lerp(startValueArmor, targetArmor, ht);
            float at = elapsedTime / duration;
            HealthSliderAnim.value = Mathf.Lerp(startValueHealth, targetHealth, at);
            yield return null;
        }
        ArmorSliderAnim.value = targetArmor;
        HealthSliderAnim.value = targetHealth;
    }
}
