﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class NickNameManager : MonoBehaviour
{
    List<Dictionary<string, object>> data;


    public InputField nickname_field;
    public Text overlapText;


    string specialStr;  //특수문자
    string nicknameStr;    //닉네임복사

    bool specialState;  //특수문자 사용 여부
    bool overlapState;  //중복상태 여부
    bool overlapOnBtn;  //중복체크 버튼 클릭 여부
    bool nullState; //닉네임 넬 체크 여부
    bool curseState;    //욕사용 여부

    //로그인 여부
    string loginState;


    void Start()
    {
        nicknameStr = "";
        specialState = true;    //초기값 true가 특수문자 사용했다는것
        curseState = true;  //초기값 true가 욕 사용했다는 것
        loginState = PlayerPrefs.GetString("AT_Player_LoginState");    //로그인 상태 여부
    }


    //닉네임 중복확인 버튼 눌렀을 때
    public void NickName_OverLap_Check()
    {
        StartCoroutine(_NickName_OverLap_Check());
    }

    IEnumerator _NickName_OverLap_Check()
    {
        overlapOnBtn = true;    //중복체크 여부(체크함)
        nicknameStr = nickname_field.text;
        nicknameStr = nicknameStr.Replace(" ", "");   //공백제거
        specialState = Special_Character_Check(nicknameStr);
        curseState = HangeulCurseCheck(nicknameStr);

        Debug.Log("욕: " + curseState);

        //닉네임이 공백일 때
        if (nicknameStr == "")
        {
            nullState = true;
            //overlapText.text = "닉네임을 입력해주시길 바랍니다.";
        }
        //닉네임이 공백이 아닐 때
        else if (nicknameStr != "")
        {
            // 닉네임 체크
            //욕설 체크
            if(curseState.Equals(true))
            {
                Debug.Log("비속어를 사용할 수 없습니다.");
            }
            //특수문자 사용
            else if (specialState == true)
            {
                Debug.Log("특수문자를 사용하지 말아주십시오");
                //overlapText.text = "특수문자를 사용하지 말아주십시오";
            }
            //특수문자 사용안함
            else if (specialState == false && nullState == false)
            {
                ServerManager.Instance.NickNameDoubleCheck(nicknameStr);

                yield return new WaitUntil(() => ServerManager.Instance.isNickNameSearchCompleted);

                ServerManager.Instance.isNickNameSearchCompleted = false;

                //서버 등록 시 열어주세요  성엽이 구간
                if (ServerManager.Instance.isExistNickName)
                {
                    Debug.Log("닉네임 중복입니다.");
                    //overlapText.text = "닉네임 중복입니다.";
                    overlapState = true;    //중복
                }
                else
                {
                    Debug.Log("사용가능한 닉네임입니다.");
                    //overlapText.text = "사용가능한 닉네임입니다.";
                    overlapState = false;   //중복아님
                }
            }
        }

        //조건식 열어주시오~ 성엽이 서버 연결했으면
        //중복체크하고, 특수문자 사용하지 않았고, 닉네임이 중복이지 않고 널값이 아니다
        if (curseState.Equals(false) && specialState == false && nullState == false && overlapState == false)
        {
            overlapText.text = "사용가능한 닉네임입니다.";
        }
        else if(curseState.Equals(true))
        {
            overlapText.text = "비속어를 사용했습니다. 다시 입력해주세요.";
        }
        //중복체크를 하지 않았다.
        else if (specialState == true)
        {
            //닉네임을 저장할 수 없다.
            overlapText.text = "특수문자를 사용했습니다. 다시 입력해주세요.";
        }
        else if (nullState == true)
        {
            overlapText.text = "닉네임을 입력하세요";
            nullState = false;
        }
        else if (overlapState == true)
        {
            overlapText.text = "닉네임 중복입니다.";
        }
        //둘중 하나라도 아닐 경우
        else
        {
            //닉네임을 저장할 수 없다.
            overlapText.text = "닉네임을 다시 설정하여 주십시오";
        }

        // 초기화
        ServerManager.Instance.isExistNickName = false;
    }


    //닉네임 특수문자 사용 여부 확인 함수
    bool Special_Character_Check(string _nickname)
    {
        specialStr = @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]";

        Regex regex = new Regex(specialStr);

        //Debug.Log(regex.IsMatch(_nickname));
        //True가 나오면 특수문자를 사용한다
        specialState = regex.IsMatch(_nickname);
        return regex.IsMatch(_nickname);
    }

    //한글욕검사
    bool HangeulCurseCheck(string _nickname)
    {
        data = CSVReader.Read("Swearlist");


        //string[] curseStr = new string[3];
        //curseStr[0] = "개새"; curseStr[1] = "멍청이"; curseStr[2] = "미친";
        //bool isCheck = false;

        //for (int i = 0; i < curseStr.Length; i++)
        //{
        //    isCheck = _nickname.Contains(curseStr[i]);

        //    if (isCheck.Equals(true))
        //        return isCheck;
        //}

        bool isCheck = false;

        //for (int i = 0; i < data.Count; i++)
        //{
        //    if(data[i]["욕"].ToString().Equals(_nickname))
        //    {
        //        isCheck = true;
        //    }
        //}

        for(int i = 0; i < data.Count; i++)
        {
            //isCheck = data[i]["욕"].ToString().Contains(_nickname);
            isCheck = _nickname.Contains(data[i]["욕"].ToString());
            if (isCheck.Equals(true))
                return isCheck;
            Debug.Log("욕 ? " + isCheck);
        }

        return isCheck;
    }

    public void NickName_Save()
    {
        //검사했던 닉네임이랑 같은지 확인
        if(nicknameStr.Equals(nickname_field.text) && nicknameStr != "")
        {
            //조건식 열어주시오~ 성엽이 서버 연결했으면
            //중복체크하고, 특수문자 사용하지 않았고, 닉네임이 중복이지 않고 널값이 아니다
            if (overlapOnBtn == true)
            {
                if (curseState.Equals(false) && specialState == false && nullState == false && overlapState == false)
                {
                    if (loginState == "GoogleNickName")
                        PlayerPrefs.SetString("AT_Player_LoginState", "GoogleCharacter");
                    else if (loginState == "GatewaysNickName")
                        PlayerPrefs.SetString("AT_Player_LoginState", "GatewaysCharacter");

                    PlayerPrefs.SetString("AT_Player_NickName", nickname_field.text);  //닉네임 저장

                    // 서버에 저장 - User INFO NickName INSERT 및 LoginState UPDATE
                    //ServerManager.Instance.UserInfo_Reg();
                    ServerManager.Instance.UserNickNameInfo_Reg();

                    SceneManager.LoadScene("CharacterChoice");   //캐릭터 성별 설정으로 이동
                }
                else
                {
                    //닉네임을 저장할 수 없다.
                    overlapText.text = "닉네임을 다시 설정하여 주십시오";
                }
            }
            //중복체크를 하지 않았다.
            else if (overlapOnBtn == false)// && (specialState == true || specialState == false || nullState == false || nullState == true))
            {
                //닉네임을 저장할 수 없다.
                overlapText.text = "닉네임 중복 체크를 해주시길 바랍니다.";
            }
            //둘중 하나라도 아닐 경우
            //else
            //{
            //    //닉네임을 저장할 수 없다.
            //    overlapText.text = "닉네임을 다시 설정하여 주십시오";
            //}
        }
        else if(nicknameStr == "")
        {
            overlapText.text = "닉네임을 설정해주세요.";
        }
        else
        {
            overlapOnBtn = false;    //중복체크 여부(체크안함)
            overlapText.text = "닉네임 중복 체크를 해주시길 바랍니다.";
        }
    }
    
}
