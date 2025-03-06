using UnityEngine;

public class WeatherController : MonoBehaviour
{

    public ParticleSystem rainSystem;



    public Light sunLight;


    public enum WeatherType { Clear, Rain, Cloudy}
    public WeatherType currentWeather = WeatherType.Clear;


    public SeedGenManager seedGenManager;
    public Material clearSkybox;
    public Material cloudySkybox;
    public Material rainSkybox;

    [Header("Light Settings - Clear")]
    public Color clearLightColor = Color.white;
    [Range(0f, 2f)]
    public float clearIntensity = 1.0f;

    [Header("Light Settings - Rain")]
    public Color rainLightColor = new Color(0.7f, 0.7f, 0.8f);
    [Range(0f, 2f)]
    public float rainIntensity = 0.6f;
    [Header("Light Settings - Cloudy")]
    public Color cloudyLightColor = new Color(0.8f, 0.8f, 0.8f);
    [Range(0f, 2f)]
    public float cloudyIntensity = 0.8f;

    [Header("Weather Cycle Settings")]
    public float weatherChangeInterval = 15f;
    private float timer = 0f;



    private void Start()
    {
        // Initialize weather
        SetWeather(WeatherType.Clear);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= weatherChangeInterval)
        {
            timer = 0f;
            int randomWeather = Random.Range(0, 3); 
            SetWeather((WeatherType)randomWeather);
        }
    }

    public void SetWeather(WeatherType newWeather)
    {
        currentWeather = newWeather;

        // Always stop rain effects before reapplying
        if (rainSystem != null) rainSystem.Stop();

        // Switch on the new weather
        switch (currentWeather)
        {
            case WeatherType.Clear:
                if (sunLight != null)
                {
                    sunLight.color = clearLightColor;
                    sunLight.intensity = clearIntensity;
                }
                break;

            case WeatherType.Rain:
                if (sunLight != null)
                {
                    sunLight.color = rainLightColor;
                    sunLight.intensity = rainIntensity;
                }
                if (rainSystem != null) rainSystem.Play();
                break;

            case WeatherType.Cloudy:
                if (sunLight != null)
                {
                    sunLight.color = cloudyLightColor;
                    sunLight.intensity = cloudyIntensity;
                }

                break;
        }

        if (seedGenManager != null)
        {
            seedGenManager.UpdateSpawnChances(newWeather);
        }
    }
}