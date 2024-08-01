using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _speed = 4.5f;
    private float _speedMultiplier = 2;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private GameObject _tripleShotPrefab;
    
    [SerializeField]
    private float _fireRate = 0.5f;
    private float _canFire = -1f;

    [SerializeField]
    public int _maxAmmo = 15;

    [SerializeField]
    private int _lives = 3;
    private SpawnManager _spawnManager;
    [SerializeField]
    private bool _isTripleShotActive = false;

    [SerializeField]
    private bool _isSpeedBoostActive = false;

    [SerializeField]
    private bool _isThrusterActive = false;

    [SerializeField]
    private GameObject _thrusterVisualizer;

    [SerializeField]
    private bool _isShieldActive = false;

    [SerializeField]
    private GameObject _shieldVisualizer;

    [SerializeField]
    private int _shieldStrength = 3;

    [SerializeField]
    private SpriteRenderer _shieldsRenderer;

    [SerializeField] 
    private List<Color> _shieldColorArray;

    [SerializeField]
    private int _score;
    private UIManager _uiManager;

    [SerializeField]
    private GameObject _leftEngineDmg;

    [SerializeField]
    private GameObject _rightEngineDmg;

    [SerializeField]
    private AudioClip _laserSoundClip;
    
    private AudioSource _audioSource;

    [SerializeField]
    AudioClip _noAmmoSound;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        _uiManager = GameObject.Find("UI_Manager").GetComponent<UIManager>();
        _audioSource = GetComponent<AudioSource>();

        _shieldsRenderer.color = _shieldColorArray[_shieldStrength -1];
        
        if (_spawnManager == null)
        {
            Debug.LogError("The Spawn Manager is NULL.");
        }

        if (_uiManager == null)
        {
            Debug.LogError("The UI Manager is Null.");
        }

        if (_audioSource == null)
        {
            Debug.LogError("The Audio Source on player is NULL.");
        }
        else
        {
            _audioSource.clip = _laserSoundClip;
        }
    }

    // Update is called once per frame
    void Update()
    {
       CalculateMovement();

         if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
         {
            if (_maxAmmo == 0)
            {
                AudioSource.PlayClipAtPoint(_noAmmoSound, transform.position);
                return;
            }
            FireLaser();
         }
       
    }

    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);

        ActivateThruster();
       
        transform.Translate(direction * GetSpeed() * Time.deltaTime);       

        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, -3.8f, 0), 0);

        if (transform.position.x > 13.5f)
        {
            transform.position = new Vector3(-13.5f, transform.position.y, 0);
        }
        else if (transform.position.x < -13.5f)
        {
            transform.position = new Vector3(13.5f, transform.position.y, 0);
        }
    }

    // LASER FIRE
    void FireLaser()
    {     
        AmmoCount(-1);
        _canFire = Time.time + _fireRate;

        if (_isTripleShotActive == true)
        {
            Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity);        
        }
        else
        {
            Instantiate(_laserPrefab, transform.position + new Vector3(0, 1.05f, 0), Quaternion.identity);
        }

        _audioSource.Play();
        
    }

    public void Damage()
    {

        //SHIELDS DAMAGE
        if (_isShieldActive == true && _shieldStrength >= 1)
        {
            _shieldStrength--;
            

            Debug.Log($"Shield strength: {_shieldStrength}");
            

            switch (_shieldStrength)
            {
                case 0:
                    _isShieldActive = false;
                    _shieldVisualizer.SetActive(false);

                    break;

                
                case 1:

                    _shieldsRenderer.color = _shieldColorArray[0];

                    break;

                case 2:
                   _shieldsRenderer.color = _shieldColorArray[1];

                    break;
                case 3:
                    _shieldsRenderer.color = _shieldColorArray[2];
                    
                    break;
                
            }
            return;
        }
        

        // LIVES DAMAGE
        _lives--;

        if (_lives == 2)
        {
            _rightEngineDmg.SetActive(true);
        }
        else if (_lives == 1)
        {
            _leftEngineDmg.SetActive(true);
        }

        _uiManager.UpdateLives(_lives);

        if (_lives < 1)
        {
            _spawnManager.OnPlayerDeath();
            Destroy(this.gameObject);
        }
    
    }

    public void TripleShotActive()
    {
        _isTripleShotActive = true;
        StartCoroutine(TripleShotPowerDownRoutine());
    }

    IEnumerator TripleShotPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isTripleShotActive = false;
    }

    public void SpeedBoostActive()
    {
        _isSpeedBoostActive = true;
        _speed *= _speedMultiplier;
        _thrusterVisualizer.SetActive(true);
        StartCoroutine(SpeedBoostPowerDownRoutine());
    }
    
    IEnumerator SpeedBoostPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isSpeedBoostActive = false;
        _speed /= _speedMultiplier;
        _thrusterVisualizer.SetActive(false);
     }

    public void ShieldActive()
    {
        _isShieldActive = true;
        _shieldVisualizer.SetActive(true);
        _shieldStrength = 3;
        _shieldsRenderer.color = _shieldColorArray[2];
    }

    public void AddScore(int points)
    {
        _score += points;
        _uiManager.UpdateScore(_score);
    }

    private float GetSpeed()
    {
        return(_speed * (_isThrusterActive ? 2.0f : 1.0f) * (_isSpeedBoostActive ? 2.0f : 1.0f));
    }

    public void ActivateThruster()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _isThrusterActive = true;
            _thrusterVisualizer.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _isThrusterActive = false;
            _thrusterVisualizer.SetActive(false);
        }
    }

    public void AmmoCount(int bullets)
    {
        if (bullets >= _maxAmmo)
        {
            _maxAmmo = 15;
        }
        else
        {
            _maxAmmo += bullets;
        }

        _uiManager.updateAmmoCount(_maxAmmo);
    }

}