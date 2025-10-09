using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class create_cible : MonoBehaviour
{
    private Camera mainCamera;
    GameObject sphere;

    public float scaleSphere = 1;
    public int distanceZ;
    public int distanceX;
    public int distanceY;

    bool aleatoireX = false;
    bool aleatoireY = false;
    bool aleatoireZ = false;

    public bool bougeX;
    public bool bougeY;
    public bool bougeZ;
    public float vitesse = 0.05f;

    public int frame_change_mouvement = 50;
    public float time_destroy = 3;

    int nombreClic = 0;
    int score = 0;

    int missedTargets = 0;
    public string situation;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI precision;
    public TextMeshProUGUI cibleToucher;

    int compter = 0;

    Vector3 bouger = new Vector3(0, 0, 0);

    public TMP_InputField inputFieldPosX;
    public TMP_InputField inputFieldPosY;
    public TMP_InputField inputFieldPosZ;
    public TMP_InputField vitCible;
    public TMP_InputField timeDestroy;
    public TMP_InputField scale;
    public TMP_InputField frame_change;

    public Toggle moveX;
    public Toggle moveY;
    public Toggle moveZ;

    int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }
        else if (value > max)
        {
            return max;
        }
        else
        {
            return value;
        }
    }


    void Start()
    {
        checkAleatoire();
        mainCamera = Camera.main;
        CreateSphere();
    }

    void checkAleatoire()
    {
        if (distanceX == 0) { aleatoireX = true; }
        else { aleatoireX = false; }
        if (distanceY == 0) { aleatoireY = true; }
        else { aleatoireY = false; }
        if (distanceZ == 0) { aleatoireZ = true; }
        else { aleatoireZ = false; }
    }

    void moveSpehere()
    {
        if (compter % frame_change_mouvement == 0)
        {
            bouger = Vector3.zero;
            if (bougeX && Mathf.Abs(sphere.transform.position.x) < 6)
            {
                while (bouger.x == 0) bouger.x = Random.Range(-1, 2) * vitesse;
            }
            if (bougeY && Mathf.Abs(sphere.transform.position.y) < 8 && (sphere.transform.position.y > 1))
            {
                while (bouger.y == 0) bouger.y = Random.Range(-1, 2) * vitesse;
            }
            if (bougeZ && Mathf.Abs(sphere.transform.position.z) < 4 && (sphere.transform.position.z > 0))
            {
                while (bouger.z == 0) bouger.z = Random.Range(-1, 2) * vitesse;
            }
        }
        //Debug.Log(bouger.x);
        sphere.transform.Translate(bouger);

    }

    void Update()
    {
        if (!(PauseMenu.isPaused))
        {
            moveSpehere();
            compter++;
            //Debug.Log("" + compter);
            // Check for mouse button press
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }
    }

    void HandleMouseClick()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("cible"))
            {
                Destroy(hit.collider.gameObject);
                score++;
                nombreClic++;
                //Debug.Log("Center of the camera is touching an object with the tag 'cible'");
                CreateSphere(); // Create a new sphere immediately after destroying the old one
            }
            else
            {
                nombreClic++;
            }
        }
        else
        {
            nombreClic++;
        }
        updateScore();
    }

    public void updateScore()
    {
        scoreText.text = "Score: " + score;
        precision.text = "Precision: " + string.Format("{0:0.##}", 100f * score / nombreClic) + "%";
        cibleToucher.text = score + "/" + (missedTargets + score);
    }

    public void CreateSphere()
    {
        float x = distanceX;
        float y = distanceY;
        float z = distanceZ;

        // x est entre -9.5 et 9.5
        // y est entre  2 et 10
        // z est entre  0.8 et 19.5
        if (aleatoireX) { x = Random.Range(-4, 4); }
        if (aleatoireY) { y = Random.Range(2, 6); }
        if (aleatoireZ) { z = Random.Range(2, 15); }

        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(x, y, z);
        sphere.tag = "cible";
        sphere.transform.localScale = new Vector3(scaleSphere, scaleSphere, scaleSphere);
        StartCoroutine(WaitDestroy(sphere));
    }

    IEnumerator WaitDestroy(GameObject currentSphere)
    {//
        if (time_destroy != 0)
        {
            yield return new WaitForSeconds(time_destroy);
            // Check if the sphere is still the current sphere before destroying
            if (currentSphere != null && currentSphere == sphere)
            {
                missedTargets++;
                Destroy(currentSphere);
                updateScore();
                CreateSphere();
            }
        }
    }



    public void updatePosX()
    {
        distanceX = int.Parse(inputFieldPosX.text);
        distanceX = Clamp(distanceX, -8, 8);
        // x est entre -9.5 et 9.5
        // y est entre  2 et 10
        // z est entre  0.8 et 19.5

        checkAleatoire();
        Debug.Log(aleatoireX);

    }
    public void updatePosY()
    {
        distanceY = int.Parse(inputFieldPosY.text);
        distanceY = Clamp(distanceY, 2, 10);
        checkAleatoire();
        Debug.Log(aleatoireY);

    }
    public void UpdatePosZ()
    {
        distanceZ = int.Parse(inputFieldPosZ.text);
        distanceZ = Clamp(distanceZ, 1, 19);
        checkAleatoire();
        Debug.Log(aleatoireZ);

    }
    public void updateScale()
    {
        scaleSphere = float.Parse(scale.text);

    }
    public void updateSpeed()
    {
        vitesse = float.Parse(vitCible.text);

    }
    public void updateFrame()
    {
        frame_change_mouvement = int.Parse(frame_change.text);

    }
    public void updateTimeDestroy()
    {
        time_destroy = float.Parse(timeDestroy.text);

    }
    public void updateBougeX()
    {
        bougeX = moveX.isOn;
    }
    public void updateBougeY()
    {
        bougeY = moveY.isOn;
    }
    public void updateBougeZ()
    {
        bougeZ = moveZ.isOn;
    }


}
