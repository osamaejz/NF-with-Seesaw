using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Ports;
using UnityEngine.UI;
using System.Threading;
using System.Net.Sockets;
using Jayrock.Json.Conversion;
using System;
using System.Text;

public class seesaaa : MonoBehaviour
{
    public Animator animator;
    private readonly int start = 0, play0 = 1, play1 = 2, play2 = 3, no_connection = 4, zero_attention = 5, exit = 6, baseline_calculation = 7;
    public Slider healthSlider;
    public Text attention_text;
    private bool check_for_button = false;
    private bool Game_over = false; // we will utilize it when we upgrade our code with timer of 120 second or other duration. 
    public string theName;
    private string threshold;
    public InputField inputField1;
    public InputField inputField2;
    public GameObject textDisplay;
    public Text score_display;
    //private int attention;
    private int threshold_level;
    private int score = 0;
    public int baseline_timer = 0;
    public bool bl_check = false;
    float current_bl_time = 10f;
    float current_game_time = 120f;
    public Text timer_text;
    public Neurosky_data att_data;
    public int attention_value = 0;
    
    [SerializeField]
    private CanvasGroup HUDCanvas;
    [SerializeField]
    private CanvasGroup Canvas01;
    [SerializeField]
    public CanvasGroup Canvas_for_timer;

    
    public void doquit()
    {
        Debug.Log("has quit game");
        Application.Quit();
    }
    public void StoreName(Button Btn)
    {
        animator = GetComponent<Animator>();
        theName = inputField1.text;
        threshold = inputField2.text;

        textDisplay.GetComponent<Text>().text = "Welcome " + theName + " to the game with level of " + threshold;
        //threshold_level = int.Parse(threshold); // use when no baseline data is recording
        check_for_button = true;
        //Debug.Log(check_2);
        ///// for invisibilty of input field and button ////
        Canvas01.alpha = 0f;
        Canvas01.interactable = false;
        Canvas01.blocksRaycasts = false;
        ////////////////////////////////////////////////////
        animator.SetInteger("statechange", baseline_calculation);
        ////// for visibility of health slider & Timer /////////////
        HUDCanvas.alpha = 1f;
        HUDCanvas.interactable = true;
        HUDCanvas.blocksRaycasts = true;

        Canvas_for_timer.alpha = 1f;
        Canvas_for_timer.interactable = true;
        Canvas_for_timer.blocksRaycasts = true;
        ////////////////////////////////////////////////////////////

    }
    public void timer_for_game()
    {
        current_game_time -= 1 * Time.deltaTime;
        timer_text.text = current_game_time.ToString("0");
        if (current_game_time == 0)
        {
            current_bl_time = 0;
            Game_over = true;
        }
    }
    public void timer_for_baseline()
    {
        current_bl_time -= 1 * Time.deltaTime;
        timer_text.text = current_bl_time.ToString("0");
        if (current_bl_time <= 0)
        {
            current_bl_time = 0;
            bl_check = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        attention_text = attention_text.GetComponent<Text>(); // SliderText
        score_display = score_display.GetComponent<Text>(); // timerText
        //Debug.Log(check_2);
        ////// for invisibility of  slider /////////////
        HUDCanvas.alpha = 0f;
        HUDCanvas.interactable = false;
        HUDCanvas.blocksRaycasts = false;
        ////////////////////////////////////////////////

        ////// for invisibility of  timer /////////////
        Canvas_for_timer.alpha = 0f;
        Canvas_for_timer.interactable = false;
        Canvas_for_timer.blocksRaycasts = false;
        ////////////////////////////////////////////////
        

    }


    // Update is called once per frame

    void Update()
    {
        //attention = threshold_level; ////// use when no headset available
        Debug.Log(Game_over);
        if (Game_over == true)
        {
            animator.SetInteger("statechange", exit);
            ////// for invisibility of  slider /////////////
            HUDCanvas.alpha = 0f;
            HUDCanvas.interactable = false;
            HUDCanvas.blocksRaycasts = false;
            ////////////////////////////////////////////////
        }
        if (check_for_button == true && Game_over == false)
        {
            attention_value = att_data.attention;
            if (attention_value > 0 && bl_check == false)
            {
                animator.SetInteger("statechange", baseline_calculation);
                baseline_timer = 1;
                timer_for_baseline();
            }
            else if (attention_value > 0 && bl_check == true)
            {
                baseline_timer = 2;
                threshold_level = att_data.Average_bl;
                timer_for_game();
                attention_value = att_data.attention;
                //Debug.Log(attention);
                healthSlider.value = attention_value;
                attention_text.text = theName + "'s Attention:" + attention_value;
                double ten_percent_baseline = threshold_level * 0.1;
                double twenty_percent_baseline = threshold_level * 0.2;
                double thirty_percent_baseline = threshold_level * 0.3;
                //Debug.Log(animator.GetInteger("statechange"));

                if ((attention_value < (threshold_level - ten_percent_baseline)) && (attention_value >= (threshold_level - thirty_percent_baseline)))
                {
                    animator.SetInteger("statechange", play0);

                }
                else if ((attention_value >= (threshold_level - ten_percent_baseline)) && (attention_value <= (threshold_level + ten_percent_baseline)))
                {
                    animator.SetInteger("statechange", play1);

                }
                else if (attention_value >= (threshold_level + ten_percent_baseline))
                {
                    animator.SetInteger("statechange", play2);
                    score += 1;
                    score_display.text = score.ToString();
                }
                else if (attention_value >= 0 && attention_value < (threshold_level - thirty_percent_baseline))
                {
                    animator.SetInteger("statechange", zero_attention);
                }
            }
            else
            {
                animator.SetInteger("statechange", no_connection);

            }

        }



    }

}

