using InstagramApiSharp.Classes;
using InstagramApiSharp.Enums;
using Minista.Views.Sign;
using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using static Helper;
// az in estefade nakardi: SendTwoFactorLoginSMSAsync

namespace Minista.ViewModels.Sign
{
    public class SignInVM : BaseModel
    {
        public SignInView View;

        //[AlsoNotifyFor("PercentageString")]
        public DispatcherTimer Timer = new DispatcherTimer();
        #region Commands

        private BaseCommand LoginCmd_;
        private BaseCommand ChallengeResendCodeCmd_;
        private BaseCommand ChallengeVerifyCodeCmd_;
        private BaseCommand ChallengeSendCodeCmd_;
        private BaseCommand SubmitChallengePhoneCmd_;
        private BaseCommand TwoFactorVerifyCodeCmd_;
        private BaseCommand ResendTwoFactorVerifyCodeCmd_;



        public BaseCommand LoginCmd
        {
            get => LoginCmd_;
            set
            {
                LoginCmd_ = value;
                OnPropertyChanged("LoginCmd");
            }
        }
        public BaseCommand ChallengeResendCodeCmd
        {
            get => ChallengeResendCodeCmd_;
            set
            {
                ChallengeResendCodeCmd_ = value;
                OnPropertyChanged("ChallengeResendCodeCmd");
            }
        }
        public BaseCommand ChallengeVerifyCodeCmd
        {
            get => ChallengeVerifyCodeCmd_;
            set
            {
                ChallengeVerifyCodeCmd_ = value;
                OnPropertyChanged("ChallengeVerifyCodeCmd");
            }
        }
        public BaseCommand ChallengeSendCodeCmd
        {
            get => ChallengeSendCodeCmd_;
            set
            {
                ChallengeSendCodeCmd_ = value;
                OnPropertyChanged("ChallengeSendCodeCmd");
            }
        }
        public BaseCommand SubmitChallengePhoneCmd
        {
            get => SubmitChallengePhoneCmd_;
            set
            {
                SubmitChallengePhoneCmd_ = value;
                OnPropertyChanged("SubmitChallengePhoneCmd");
            }
        }
        public BaseCommand TwoFactorVerifyCodeCmd
        {
            get => TwoFactorVerifyCodeCmd_;
            set
            {
                TwoFactorVerifyCodeCmd_ = value;
                OnPropertyChanged("TwoFactorVerifyCodeCmd");
            }
        }
        public BaseCommand ResendTwoFactorVerifyCodeCmd
        {
            get => ResendTwoFactorVerifyCodeCmd_;
            set
            {
                ResendTwoFactorVerifyCodeCmd_ = value;
                OnPropertyChanged("ResendTwoFactorVerifyCodeCmd");
            }
        }
        #endregion Commands

        #region Visibilies
        private Visibility LoginGridVisibility_ = Visibility.Visible;
        private Visibility TwoFactorGridVisibility_ = Visibility.Collapsed;
        private Visibility Challenge1GridVisibility_ = Visibility.Collapsed;
        private Visibility Challenge2GridVisibility_ = Visibility.Collapsed;
        private Visibility Challenge3GridVisibility_ = Visibility.Collapsed;
        private Visibility FacebookGridVisibility_ = Visibility.Collapsed;
        private Visibility ChallengePhoneNumberRadioVisibility_ = Visibility.Collapsed;
        private Visibility ChallengeEmailRadioVisibility_ = Visibility.Collapsed;
        private Visibility LoadingGridVisibility_ = Visibility.Collapsed;
        private Visibility ChallengeV2LoadingGridVisibility_ = Visibility.Collapsed;
        
        public Visibility LoginGridVisibility
        {
            get => LoginGridVisibility_;
            set
            {
                LoginGridVisibility_ = value;
                OnPropertyChanged("LoginGridVisibility");
            }
        }
        public Visibility TwoFactorGridVisibility
        {
            get => TwoFactorGridVisibility_;
            set
            {
                TwoFactorGridVisibility_ = value;
                OnPropertyChanged("TwoFactorGridVisibility");
            }
        }
        public Visibility Challenge1GridVisibility
        {
            get => Challenge1GridVisibility_;
            set
            {
                Challenge1GridVisibility_ = value;
                OnPropertyChanged("Challenge1GridVisibility");
            }
        }
        public Visibility Challenge2GridVisibility
        {
            get => Challenge2GridVisibility_;
            set
            {
                Challenge2GridVisibility_ = value;
                OnPropertyChanged("Challenge2GridVisibility");
            }
        }
        public Visibility Challenge3GridVisibility
        {
            get => Challenge3GridVisibility_;
            set
            {
                Challenge3GridVisibility_ = value;
                OnPropertyChanged("Challenge3GridVisibility");
            }
        }
        public Visibility FacebookGridVisibility
        {
            get => FacebookGridVisibility_;
            set
            {
                FacebookGridVisibility_ = value;
                OnPropertyChanged("FacebookGridVisibility");
            }
        }


        public Visibility ChallengePhoneNumberRadioVisibility
        {
            get => ChallengePhoneNumberRadioVisibility_;
            set
            {
                ChallengePhoneNumberRadioVisibility_ = value;
                OnPropertyChanged("ChallengePhoneNumberRadioVisibility");
            }
        }
        public Visibility ChallengeEmailRadioVisibility
        {
            get => ChallengeEmailRadioVisibility_;
            set
            {
                ChallengeEmailRadioVisibility_ = value;
                OnPropertyChanged("ChallengeEmailRadioVisibility");
            }
        }
        public Visibility LoadingGridVisibility
        {
            get => LoadingGridVisibility_;
            set
            {
                LoadingGridVisibility_ = value;
                OnPropertyChanged("LoadingGridVisibility");
            }
        }
        public Visibility ChallengeV2LoadingGridVisibility
        {
            get => ChallengeV2LoadingGridVisibility_;
            set
            {
                ChallengeV2LoadingGridVisibility_ = value;
                OnPropertyChanged("ChallengeV2LoadingGridVisibility");
            }
        }
        #endregion Visibilies

        #region Properties
        private string Username_;
        private string Password_;
        private string ChallengeVerificationCode_;
        private string TwoFactorVerificationCode_;
        private bool LoadingRingIsActive_ = false;
        private bool ChallengeV2LoadingRingIsActive_ = false;
 
        public string Username { get { return Username_; } set { Username_ = value; OnPropertyChanged("Username"); } }
        public string Password { get { return Password_; } set { Password_ = value; OnPropertyChanged("Password"); } }
        public string ChallengeVerificationCode { get { return ChallengeVerificationCode_; } set { ChallengeVerificationCode_ = value; OnPropertyChanged("ChallengeVerificationCode"); } }
        public string TwoFactorVerificationCode { get { return TwoFactorVerificationCode_; } set { TwoFactorVerificationCode_ = value; OnPropertyChanged("TwoFactorVerificationCode"); } }
        public bool LoadingRingIsActive { get { return LoadingRingIsActive_; } set { LoadingRingIsActive_ = value; OnPropertyChanged("LoadingRingIsActive"); } }
        public bool ChallengeV2LoadingRingIsActive { get { return ChallengeV2LoadingRingIsActive_; } set { ChallengeV2LoadingRingIsActive_ = value; OnPropertyChanged("ChallengeV2LoadingRingIsActive"); } }
        

        private int _interval = 60;
        public int Interval { get { return _interval; } set { _interval = value; OnPropertyChanged("Interval"); } }

        private bool _trustThisDevice = true;
        public bool TrustThisDevice { get { return _trustThisDevice; } set { _trustThisDevice = value; OnPropertyChanged("TrustThisDevice"); } }

        #endregion Properties

        #region Constructor
        public SignInVM()
        {
            LoginCmd = BaseCommand.GetInstance();
            ChallengeVerifyCodeCmd = BaseCommand.GetInstance();
            ChallengeResendCodeCmd = BaseCommand.GetInstance();
            ChallengeSendCodeCmd = BaseCommand.GetInstance();
            SubmitChallengePhoneCmd = BaseCommand.GetInstance();
            TwoFactorVerifyCodeCmd = BaseCommand.GetInstance();
            ResendTwoFactorVerifyCodeCmd = BaseCommand.GetInstance();

            LoginCmd.ExecuteFunc = Login;
            SubmitChallengePhoneCmd.ExecuteFunc = SubmitChallengePhone;
            ChallengeSendCodeCmd.ExecuteFunc = ChallengeSendCode;
            ChallengeResendCodeCmd.ExecuteFunc = ChallengeResendCode;
            ChallengeVerifyCodeCmd.ExecuteFunc = ChallengeVerifyCode;
            TwoFactorVerifyCodeCmd.ExecuteFunc = TwoFactorVerifyCode;
            ResendTwoFactorVerifyCodeCmd.ExecuteFunc = ResendTwoFactorVerifyCode;

            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += TimerTick;
        }
        #endregion Constructor
        public async void Login(object obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, LoginAsync);
        }

        public async void BeforeLogin()
        {
            try
            {
                InstaApiTrash = BuildApi("", "");
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await InstaApiTrash.SendRequestsBeforeLoginAsync());
            }
            catch { }
        }
        public async void AfterLogin()
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await InstaApi.SendRequestsAfterLoginAsync());
            }
            catch { }
        }
        public async void LoginAsync()
        {
            if (string.IsNullOrEmpty(Username))
            {
                try
                {
                    View.UsernameText.Focus(FocusState.Keyboard);
                }
                catch { }
                MainPage.Current.ShowInAppNotify("Please type your username, email or phone number.", 1000);
                return;
            }
            if (string.IsNullOrEmpty(Password))
            {
                View.PasswordText.Focus(FocusState.Keyboard);
                MainPage.Current.ShowInAppNotify("Please type your password.", 1000);
                return;
            }
            //try
            {
                var user = Username.Trim();
                var pass = Password;
                if (InstaApiTrash == null)
                    InstaApiTrash = BuildApi(user, pass);

                InstaApiTrash.SetUser(UserSessionData.ForUsername(user).WithPassword(pass));

                $"{user} - Connecting...".ChangeAppTitle();
                MainPage.Current.ShowLoading();
                var loginResult = await InstaApiTrash.LoginAsync();
                HandleLogin(loginResult);
            }
            //catch (Exception ex) { ex.PrintException("LoginButtonClick"); }
        }
        public async void HandleLogin(IResult<InstaLoginResult> loginResult)
        {
            try
            {
                LoadingOff();
                if (loginResult.Succeeded)
                {
                    MainPage.Current.HideLoading();
                    //AddInstaApi(InstaApi);
                    InstaApi = InstaApiTrash;
                    //AddInstaApi(InstaApi);

                    InstaApiSelectedUsername = InstaApi.GetLoggedUser().LoggedInUser.UserName.ToLower();
                    Connected();
                    AfterLogin();
                    InstaApiTrash = null;
                    MainPage.Current?.SetStackPanelTitleVisibility(Visibility.Visible);
                    SessionHelper.SaveCurrentSession();
                }
                else
                {
                    MainPage.Current.HideLoading();
                    switch (loginResult.Value)
                    {
                        case InstaLoginResult.InvalidUser:
                            "".ChangeAppTitle();
                            Helper.ShowMsg("Username is invalid.", "ERR");
                            break;
                        case InstaLoginResult.BadPassword:
                            "".ChangeAppTitle();
                            Helper.ShowMsg("Password is wrong.", "ERR");
                            break;
                        case InstaLoginResult.Exception:
                            "".ChangeAppTitle();
                            Helper.ShowMsg("Exception throws:\n" + loginResult.Info?.Message, "ERR");
                            break;
                        case InstaLoginResult.LimitError:
                            "".ChangeAppTitle();
                            Helper.ShowMsg("Limit error (you should wait 10 minutes).", "ERR");
                            break;
                        case InstaLoginResult.ChallengeRequired:
                            "".ChangeAppTitle();
                            TwoFactorGridVisibility = Visibility.Collapsed;
                            LoginGridVisibility = Visibility.Collapsed;
                            HandleChallenge();
                            break;
                        case InstaLoginResult.TwoFactorRequired:
                            "".ChangeAppTitle();
                            //Interval = 60;
                            //View.txtInterval.Visibility = Visibility.Visible;
                            //Timer.Start();
                            var twoFactor = await InstaApiTrash.GetTwoFactorInfoAsync();
                            if (twoFactor.Succeeded)
                            {
                                View.AddTwoFactorOptions(twoFactor.Value.SmsTwoFactorOn ?? true, twoFactor.Value.ToTpTwoFactorOn ?? false);
                            }
                            LoginGridVisibility = Visibility.Collapsed;
                            TwoFactorGridVisibility = Visibility.Visible;
                            break;
                        case InstaLoginResult.Success: break;
                        default:
                            if (loginResult.Info != null)
                                if (!loginResult.Info.Message.ToLower().Contains("no errors detected"))
                                    loginResult.Info?.Message?.ShowErr(loginResult.Info.Exception);
                            break;
                    }
                }
            }
            catch { }
        }
        public void StartTimer(bool flag = false)
        {
            try
            {
                if (flag)
                {
                    View.ResendTwoFactorVerifyCodeButton.IsEnabled = false;
                    Interval = 60;
                    View.txtInterval.Visibility = Visibility.Visible;
                    Timer.Start();
                }
                else
                    View.ResendTwoFactorVerifyCodeButton.IsEnabled = true;
            }
            catch { }
        }
        public void StopTimer()
        {
            try
            {
                View.txtInterval.Visibility = Visibility.Collapsed;
                Timer.Stop();
            }
            catch { }
        }
        public void Connected()
        {
            try
            {
                var apis = InstaApiList.ToList();
                apis.AddInstaApiX(InstaApi);
                Helpers.MultiApiHelper.SetupPushNotification(apis);
                MainPage.Current?.NavigateToMainView(true);

                //return;
                //if (InstaApi != null && InstaApi.GetLoggedUser() != null &&
                //    !string.IsNullOrEmpty(InstaApi.GetLoggedUser().UserName))
                //{
                //    MainPage.Current.SetUserAndPicture();
                //    var user = await InstaApi.GetCurrentUserAsync();
                //    //Helpers.NavigationService.Navigate(typeof(MainPage));
                //    //Helpers.NavigationService.Navigate(typeof(Views.Main.MainView));
                //    MainPage.Current?.NavigateToMainView();
                //    if (!user.Succeeded && user.Info.ResponseType != ResponseType.NetworkProblem)
                //    {
                //        if (user.Info.ResponseType == ResponseType.LoginRequired)
                //            Logoff(true);
                //        return;
                //    }
                //    MainPage.Current?.SetStackPanelTitleVisibility(Visibility.Visible);
                //    if (user.Succeeded)
                //    {
                //        InstaApi.GetLoggedUser().LoggedInUser.IsPrivate = user.Value.IsPrivate;
                //        InstaApi.GetLoggedUser().LoggedInUser.IsVerified = user.Value.IsVerified;
                //        InstaApi.GetLoggedUser().LoggedInUser.ProfilePicture = user.Value.ProfilePicture;
                //        InstaApi.GetLoggedUser().LoggedInUser.ProfilePictureId = user.Value.ProfilePictureId;
                //        InstaApi.GetLoggedUser().LoggedInUser.ProfilePicUrl = user.Value.ProfilePicUrl;
                //        InstaApi.GetLoggedUser().LoggedInUser.UserName = user.Value.UserName;
                //        InstaApi.GetLoggedUser().LoggedInUser.FullName = user.Value.FullName;
                //    }
                //    MainPage.Current.SetUserAndPicture();
                //    //LoginText.Text = $"Logged in as '{username}'";
                //    //LoginGrid.Visibility = Visibility.Collapsed;
                //}
                //else
                //{

                //    View.Visibility = Visibility.Visible;
                //}
            }
            catch
            {
                try
                {
                    View.Visibility = Visibility.Visible;
                }
                catch { }
            }
        }
        public async void Logoff(bool needsRelogin = false)
        {
            try
            {
                if (needsRelogin)
                    $"It seems you were logged out from this account [{InstaApiSelectedUsername}], or your password is changed.\r\nRe-login is required.".ShowMsg("");
                MainPage.Current.ShowLoading();
                try
                {
                    await InstaApi.LogoutAsync();
                }
                catch { }
                foreach(var item in InstaApiList.ToList())
                {
                    if(item.GetLoggedUser().UserName.ToLower() == InstaApiSelectedUsername.ToLower())
                    {
                        InstaApiList.Remove(item);
                        break;
                    }
                }
                InstaApi = null;
                InstaApiSelectedUsername = null;
                SettingsHelper.SaveSettings();
                SessionHelper.DeleteCurrentSession();
                "".ChangeAppTitle();

                MainPage.Current.HideLoading();
                //LoginGrid.Visibility = Visibility.Visible;
                //Visibility = Visibility.Visible;
                if (Helpers.NavigationService.Frame.BackStack.Any())
                    Helpers.NavigationService.Frame.BackStack.Clear();
            }
            catch
            {
                MainPage.Current.HideLoading();
            }
        }


        #region challenge require functions

        private async void SubmitChallengePhone(object obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, SubmitChallengePhoneAsync);
        }

        async void HandleChallenge(bool resend = false)
        {
            try
            {
                MainPage.Current.ShowLoading();
                IResult<InstaChallengeRequireVerifyMethod> challenge = null;
                if (!resend)
                    challenge = await InstaApiTrash.GetChallengeRequireVerifyMethodAsync();
                else
                    challenge = await InstaApiTrash.ResetChallengeRequireVerifyMethodAsync();

                MainPage.Current.HideLoading();
                if (challenge.Succeeded)
                {
                    if (challenge.Value.SubmitPhoneRequired)
                        Challenge3GridVisibility = Visibility.Visible;
                    else
                    {
                        if (challenge.Value.StepData != null)
                        {
                            ChallengePhoneNumberRadioVisibility =
                                ChallengeEmailRadioVisibility = Visibility.Collapsed;
                            if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
                            {
                                if (!resend)
                                    View.ChallengePhoneNumberRadio.IsChecked = false;
                                ChallengePhoneNumberRadioVisibility = Visibility.Visible;
                                View.ChallengePhoneNumberRadio.Content = challenge.Value.StepData.PhoneNumber;
                            }
                            if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
                            {
                                if (!resend)
                                    View.ChallengeEmailRadio.IsChecked = false;
                                ChallengeEmailRadioVisibility = Visibility.Visible;
                                View.ChallengeEmailRadio.Content = challenge.Value.StepData.Email;
                            }
                            if (!resend)
                                Challenge1GridVisibility = Visibility.Visible;
                        }
                    }
                }
                else if(challenge.Info.ResponseType == ResponseType.ChallengeRequiredV2 || challenge.Info.ResponseType == ResponseType.ChallengeRequired)
                    View.StartChallengeV2(InstaApiTrash.ChallengeLoginInfo);
                else
                    Helper.ShowMsg(challenge.Info.Message, "ERR");
            }
            catch (Exception ex)
            {
                ex.PrintException("HandleChallenge");
                MainPage.Current.HideLoading();
            }
        }

        private async void SubmitChallengePhoneAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(View.SubmitChallengePhoneText.Text) ||
                     string.IsNullOrWhiteSpace(View.SubmitChallengePhoneText.Text))
                {
                    Helper.ShowMsg("Please type a valid phone number(with country code).\r\ni.e: +989123456789", "ERR");
                    return;
                }
                MainPage.Current.ShowLoading();
                var phoneNumber = View.SubmitChallengePhoneText.Text.Trim();
                if (!phoneNumber.StartsWith("+"))
                    phoneNumber = $"+{phoneNumber}";

                var submitPhone = await InstaApiTrash.SubmitPhoneNumberForChallengeRequireAsync(phoneNumber);

                MainPage.Current.HideLoading();
                if (submitPhone.Succeeded)
                {
                    Challenge3GridVisibility = Visibility.Collapsed;
                    Challenge2GridVisibility = Visibility.Visible;
                }
                else
                    Helper.ShowMsg(submitPhone.Info.Message, "ERR");

            }
            catch (Exception ex)
            {
                Helper.ShowMsg(ex.Message, "EX");
                MainPage.Current.HideLoading();
            }
        }

        private async void ChallengeSendCode(object obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ChallengeSendCodeAsync);
        }
        private async void ChallengeSendCodeAsync()
        {
            if (InstaApiTrash == null)
                return;
            bool isEmail = false;
            if (View.ChallengeEmailRadio.IsChecked.Value)
                isEmail = true;
            try
            {
                MainPage.Current.ShowLoading();
                if (isEmail)
                {
                    var email = await InstaApiTrash.RequestVerifyCodeToEmailForChallengeRequireAsync();

                    MainPage.Current.HideLoading();
                    if (email.Succeeded)
                    {
                        View.SmsEmailText.Text = $"We sent verify code to this email:\n{email.Value.StepData.ContactPoint}";
                        Challenge1GridVisibility = Visibility.Collapsed;
                        Challenge2GridVisibility = Visibility.Visible;
                    }
                    else
                        Helper.ShowMsg(email.Info.Message, "ERR");
                }
                else
                {
                    var phoneNumber = await InstaApiTrash.RequestVerifyCodeToSMSForChallengeRequireAsync();
                    MainPage.Current.HideLoading();
                    if (phoneNumber.Succeeded)
                    {
                        View.SmsEmailText.Text = $"We sent verify code to this phone number(it's end with this):\n{phoneNumber.Value.StepData.ContactPoint}";
                        Challenge1GridVisibility = Visibility.Collapsed;
                        Challenge2GridVisibility = Visibility.Visible;
                    }
                    else
                        Helper.ShowMsg(phoneNumber.Info.Message, "ERR");
                }
            }
            catch (Exception ex)
            {
                Helper.ShowMsg(ex.Message, "ERR");
                ex.PrintException("ChallengeSendCodeButtonClick");
                MainPage.Current.HideLoading();
            }

        }

        private async void ChallengeResendCode(object obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ChallengeResendCodeAsync);
        }
        private async void ChallengeResendCodeAsync()
        {
            if (InstaApiTrash == null)
                return;
            try
            {
                MainPage.Current.ShowLoading();
                var reset = await InstaApiTrash.ResetChallengeRequireVerifyMethodAsync();
                MainPage.Current.HideLoading();
                reset.Succeeded.PrintDebug();
                HandleChallenge(true);
            }
            catch (Exception ex)
            {
                Helper.ShowMsg(ex.Message, "ERR");
                ex.PrintException("ChallengeResendCodeButtonClick");
                MainPage.Current.HideLoading();
            }
        }

        private async void ChallengeVerifyCode(object obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ChallengeVerifyCodeAsync);
        }
        private async void ChallengeVerifyCodeAsync()
        {
            if (InstaApiTrash == null)
                return;
            try
            {
                ChallengeVerificationCode = ChallengeVerificationCode.Trim();
                ChallengeVerificationCode = ChallengeVerificationCode.Replace(" ", "");
                var regex = new Regex(@"^-*[0-9,\.]+$");
                if (!regex.IsMatch(ChallengeVerificationCode))
                {
                    Helper.ShowMsg("Verification code is numeric!!!", "ERR");
                    return;
                }
                if (ChallengeVerificationCode.Length != 6)
                {
                    Helper.ShowMsg("Verification code must be 6 digits!!!", "ERR");
                    return;
                }

                MainPage.Current.ShowLoading();
                var verify = await InstaApiTrash.VerifyCodeForChallengeRequireAsync(ChallengeVerificationCode);
                MainPage.Current.HideLoading();
                if (verify.Succeeded)
                {
                    //AddInstaApi(InstaApi);
                    InstaApi = InstaApiTrash;
                    //AddInstaApi(InstaApi);
                    InstaApiSelectedUsername = InstaApi.GetLoggedUser().LoggedInUser.UserName.ToLower();
                    Connected();
                    AfterLogin();
                    InstaApiTrash = null;
                    Challenge1GridVisibility = Challenge2GridVisibility = Visibility.Collapsed;
                    ChallengeVerificationCode = "";
                    SessionHelper.SaveCurrentSession();
                    Connected();
                    return;
                }
                else
                {
                    if (verify.Value == InstaLoginResult.TwoFactorRequired)
                    {
                        LoginGridVisibility = Challenge1GridVisibility = Challenge2GridVisibility = Visibility.Collapsed;
                        ChallengeVerificationCode = "";

                        TwoFactorVerificationCode = "";
                        "".ChangeAppTitle();
                        TwoFactorGridVisibility = Visibility.Visible;
                    }
                }
                Helper.ShowMsg(verify.Info.Message, "ERR");
            }
            catch (Exception ex)
            {
                ex.PrintException("ChallengeVerifyCodeButtonClick");
                MainPage.Current.HideLoading();
            }
        }

        #endregion challenge require functions


        #region Two factor authentication
        public InstaTwoFactorVerifyOptions TwoFactorVerifyOptions = InstaTwoFactorVerifyOptions.SmsCode;

        private async void TwoFactorVerifyCode(object obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, TwoFactorVerifyCodeAsync);
        }
        private async void TwoFactorVerifyCodeAsync()
        {
            if (InstaApiTrash == null)
                return;
            if (string.IsNullOrEmpty(TwoFactorVerificationCode))
            {
                Helper.ShowMsg("Please write your two factor code and then press Auth button.", "ERR");
                return;
            }
            try
            {
                MainPage.Current.ShowLoading();
                // send two factor code
                IResult<InstaLoginTwoFactorResult> twoFactorLogin;
                //if (IsFacebookTwoFactorEnabled)
                //    twoFactorLogin = await Helper.InstaApiTrash.TwoFactorLoginFacebookAsync(TwoFactorVerificationCodeText.Text);
                //else
                twoFactorLogin = await InstaApiTrash.TwoFactorLoginAsync(TwoFactorVerificationCode, TrustThisDevice, TwoFactorVerifyOptions);
                MainPage.Current.HideLoading();
                if (twoFactorLogin.Succeeded)
                {
                    if (twoFactorLogin.Info.Message.Contains("Challenge is required"))
                        HandleLogin(Result.Fail("", InstaLoginResult.ChallengeRequired));
                    else
                        HandleLogin(Result.Success(InstaLoginResult.Success));
                    //InstaApi = InstaApiTrash;
                    //InstaApiSelectedUsername = InstaApi.GetLoggedUser().LoggedInUser.UserName.ToLower();
                    //AddInstaApi(InstaApi);
                    //Connected();
                    //AfterLogin();
                    //InstaApiTrash = null;
                    //MainPage.Current?.SetStackPanelTitleVisibility(Visibility.Visible);
                    //SessionHelper.SaveCurrentSession();
                    //TwoFactorGridVisibility = Visibility.Collapsed;
                    //TwoFactorVerificationCode = "";
                }
                else
                {
                    if (twoFactorLogin.Info.Message.Contains("Challenge is required"))
                        HandleLogin(Result.Fail("", InstaLoginResult.ChallengeRequired));
                    else
                    {
                        ResendTwoFactorVerifyCode(true);
                        Helper.ShowMsg(twoFactorLogin.Info.Message, "ERR");
                    }
                }

            }
            catch (Exception ex)
            {
                ex.PrintException("TwoFactorVerifyCodeButtonClick");
                MainPage.Current.HideLoading();
            }
        }


        private async void ResendTwoFactorVerifyCode(object obj)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()=> ResendTwoFactorVerifyCodeAsync(obj));
        }
        private async void ResendTwoFactorVerifyCodeAsync(object obj)
        {
            if (InstaApiTrash == null)
                return;
            var isBool = false;
            try
            {
                if (obj is bool b)
                    isBool = true;
            }
            catch { }
            try
            {

                MainPage.Current.ShowLoading();
                var resendTwoFactorLogin = await InstaApiTrash.SendTwoFactorLoginSMSAsync();
                MainPage.Current.HideLoading();
                if (resendTwoFactorLogin.Succeeded)
                {
                    TwoFactorVerificationCode = "";
                    Interval = 60;
                    Timer.Start();
                    if (!isBool)
                        $"We sent verification code to your phone number that ends with '{resendTwoFactorLogin.Value?.TwoFactorInfo?.ObfuscatedPhoneNumber}'".ShowMsg();
                    else
                    {
                        await Task.Delay(1500);
                        "We sent you a fresh verification code".ShowMsg();
                    }
                }
                else
                {
                    if (!isBool)
                        Helper.ShowMsg(resendTwoFactorLogin.Info.Message, "ERR");
                }
            }
            catch (Exception ex)
            {
                ex.PrintException("TwoFactorVerifyCodeButtonClick");
                MainPage.Current.HideLoading();
            }
        }
        #endregion


        private void TimerTick(object sender, object e)
        {
            try
            {
                Interval--;
                if (Interval == 0)
                {
                    View.txtInterval.Visibility = Visibility.Collapsed;
                    View.ResendTwoFactorVerifyCodeButton.IsEnabled = true;
                    Timer.Stop();
                }
            }
            catch { }
        }




        #region Loading
        public void LoadingOn()
        {
            LoadingRingIsActive = true;
            LoadingGridVisibility = Visibility.Visible;
        }
        public void LoadingOff()
        {
            LoadingRingIsActive = false;
            LoadingGridVisibility = Visibility.Collapsed;
        }


        public void ChallengeV2LoadingOn()
        {
            ChallengeV2LoadingRingIsActive = true;
            ChallengeV2LoadingGridVisibility = Visibility.Visible;
        }
        public void ChallengeV2LoadingOff()
        {
            ChallengeV2LoadingRingIsActive = false;
            ChallengeV2LoadingGridVisibility = Visibility.Collapsed;
        }

        #endregion Loading
    }
}
