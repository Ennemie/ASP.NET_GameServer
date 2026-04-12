using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using System; // Cần thêm cái này để dùng Action

public class CanvasController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject opening;
    public GameObject loginHub;
    public GameObject registerHub;
    public GameObject loggedInHub;
    public GameObject leaderBoardHub;
    public GameObject shopHub;
    [SerializeField] private VisualTreeAsset shopItemTemplate;

    private UIDocument loginUI;
    private UIDocument registerUI;
    private UIDocument loggedInUI;
    private UIDocument leaderBoardUI;
    private UIDocument shopUI;

    //logged in
    private int loggedInID;
    private int loggedInWalletBalance;
    private string loggedInName;
    private string loggedInEmail;
    private string token;

    private Label greeting;
    private Button leaderBoardBtn;
    private Button shopBtn;
    private Button logOutBtn;

    // Login
    private Button loginBackBtn;
    private Button loginBtn;
    private Button goToRegisterBtn;
    private TextField login_emailInput;
    private TextField login_passwordInput;
    private Label login_status;

    //Register
    private Button registerBackBtn;
    private Button registerBtn;
    private TextField register_charNameInput;
    private TextField register_emailInput;
    private TextField register_passwordInput;
    private DropdownField register_gameModeName;
    private Label register_status;

    // Leaderboard
    private ScrollView scrollView;
    private Button ldbCloseBtn;

    //Shop Variables
    private Button shopCloseBtn;
    private Label walletText;
    private Button ItemBtn;
    private Button vehicleBtn;
    private Label shopStatus;
    private Button buyBtn;
    private ListView shopList;

    // List lưu data
    private List<ItemVehicleModel> _allProducts = new List<ItemVehicleModel>();
    private List<ItemVehicleModel> _currentList = new List<ItemVehicleModel>();

    private string loginUrl = "http://localhost:6969/LoginRequest";
    private string registerUrl = "http://localhost:6969/RegisterRequest";
    private string getAllPlayerUrl = "http://localhost:6969/GetAllPlayer";
    private string getItemVehicleUrl = "http://localhost:6969/GetItemVehicleList";
    private string purchaseUrl = "http://localhost:6969/PostPurchase";

    private void Start()
    {
        loginUI = loginHub.GetComponent<UIDocument>();
        registerUI = registerHub.GetComponent<UIDocument>();
        loggedInUI = loggedInHub.GetComponent<UIDocument>();
        leaderBoardUI = leaderBoardHub.GetComponent<UIDocument>();
        shopUI = shopHub.GetComponent<UIDocument>();
        OpenOpening();
    }

    // Mở Opening
    private void OpenOpening()
    {
        loggedInID = 0;
        loggedInWalletBalance = 0;
        loggedInName = "";
        loggedInEmail = "";
        token = "";

        opening.SetActive(true);
        loginHub.SetActive(false);
        registerHub.SetActive(false);
        loggedInHub.SetActive(false);
        leaderBoardHub.SetActive(false);
        shopHub.SetActive(false);
    }

    // --- PHẦN LOGIN (GIỮ NGUYÊN) ---
    public void OpenLoginHub()
    {
        opening.SetActive(false);
        loginHub.SetActive(true);
        registerHub.SetActive(false);
        loggedInHub.SetActive(false);
        leaderBoardHub.SetActive(false);
        shopHub.SetActive(false);
        StartCoroutine(SetUpLoginHub());
    }
    IEnumerator SetUpLoginHub()
    {
        yield return null;
        var loginRoot = loginUI.rootVisualElement;

        login_emailInput = loginRoot.Q<TextField>("input-email");
        login_passwordInput = loginRoot.Q<TextField>("input-password");
        login_emailInput.isDelayed = true;
        login_passwordInput.isDelayed = true;
        login_status = loginRoot.Q<Label>("lbl-status");

        loginBackBtn = loginRoot.Q<Button>("btn-back");
        loginBackBtn.clicked -= OpenOpening; // Tránh cộng dồn
        loginBackBtn.clicked += OpenOpening;

        goToRegisterBtn = loginRoot.Q<Button>("btn-goto-register");
        goToRegisterBtn.clicked -= OpenRegisterHub;
        goToRegisterBtn.clicked += OpenRegisterHub;

        loginBtn = loginRoot.Q<Button>("btn-login");
        loginBtn.clicked -= OnLoginClicked; // Tách hàm để dễ quản lý event
        loginBtn.clicked += OnLoginClicked;
    }

    private void OnLoginClicked() => StartCoroutine(LoginProcess());

    private IEnumerator LoginProcess()
    {
        if (string.IsNullOrEmpty(login_emailInput.value))
        {
            login_status.text = "Vui lòng nhập Email!";
            yield break;
        }
        else if (string.IsNullOrEmpty(login_passwordInput.value))
        {
            login_status.text = "Vui lòng nhập Password!";
            yield break;
        }
        else
        {
            login_status.text = "Đang đăng nhập...";
        }
        var loginData = new LoginModel(login_emailInput.value, login_passwordInput.value);
        string jsonLoginData = JsonUtility.ToJson(loginData);

        using (UnityWebRequest www = new UnityWebRequest(loginUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonLoginData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                login_status.text = "Đăng nhập thành công!";
                HandleResponse(www.responseCode.ToString(), www.downloadHandler.text);
                loggedInEmail = login_emailInput.value;
                Debug.Log("Logged in: " + loggedInEmail);
                yield break;
            }
            else
            {
                login_status.text = HandleResponse(www.responseCode.ToString(), www.downloadHandler.text);
                loggedInEmail = "";
                yield break;
            }
        }
    }
    private string HandleResponse(string codeResponse, string jsonResponse)
    {
        if (codeResponse == "0") return "Không thể kết nối đến Server!";
        if (string.IsNullOrEmpty(jsonResponse)) return "Dữ liệu từ Server không hợp lệ!";

        ResponseModel response = null;
        try
        {
            response = JsonUtility.FromJson<ResponseModel>(jsonResponse);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi parse JSON: {ex.Message}");
            return "Không thể đọc dữ liệu từ Server!";
        }

        if (response == null) return "Dữ liệu từ Server lỗi!";

        Debug.Log($"Code: {codeResponse}: {response.notification}");
        if (response.isSuccess && !string.IsNullOrEmpty(response.token))
        {
            token = response.token;
            loggedInName = response.loggedInName;
            loggedInID = response.playerID;
            loggedInWalletBalance = response.walletBalance;
            StartCoroutine(OpenLoggedInHub());
        }
        return response.notification ?? "Không có thông báo từ Server!";
    }

    // --- PHẦN REGISTER (GIỮ NGUYÊN) ---
    private void OpenRegisterHub()
    {
        opening.SetActive(false);
        registerHub.SetActive(true);
        loginHub.SetActive(false);
        loggedInHub.SetActive(false);
        StartCoroutine(SetUpRegisterHub());
    }
    IEnumerator SetUpRegisterHub()
    {
        yield return null;
        var registerRoot = registerUI.rootVisualElement;

        register_charNameInput = registerRoot.Q<TextField>("input-charname");
        register_emailInput = registerRoot.Q<TextField>("input-email");
        register_passwordInput = registerRoot.Q<TextField>("input-password");
        register_gameModeName = registerRoot.Q<DropdownField>("drp-gamemode");
        register_status = registerRoot.Q<Label>("lbl-status");

        register_charNameInput.isDelayed = true;
        register_emailInput.isDelayed = true;
        register_passwordInput.isDelayed = true;

        registerBackBtn = registerRoot.Q<Button>("btn-back");
        registerBackBtn.clicked -= OpenOpening;
        registerBackBtn.clicked += OpenOpening;

        registerBtn = registerRoot.Q<Button>("btn-register");
        registerBtn.clicked -= OnRegisterClicked;
        registerBtn.clicked += OnRegisterClicked;
    }
    private void OnRegisterClicked() => StartCoroutine(RegisterProcess());

    private IEnumerator RegisterProcess()
    {
        if (string.IsNullOrEmpty(register_charNameInput.value))
        {
            register_status.text = "Vui lòng nhập tên nhân vật!";
            yield break;
        }
        else if (string.IsNullOrEmpty(register_emailInput.value))
        {
            register_status.text = "Vui lòng nhập Email!";
            yield break;
        }
        else if (string.IsNullOrEmpty(register_passwordInput.value))
        {
            register_status.text = "Vui lòng nhập Password!";
            yield break;
        }
        else
        {
            register_status.text = "Đang đăng ký...";
        }

        int gameModeID;
        if (register_gameModeName.value == "Survival") gameModeID = 1;
        else if (register_gameModeName.value == "Creative") gameModeID = 2;
        else gameModeID = 3;

        var registerData = new RegisterModel(register_charNameInput.value, register_emailInput.value, register_passwordInput.value, gameModeID);
        string jsonRegisterData = JsonUtility.ToJson(registerData);

        using (UnityWebRequest www = new UnityWebRequest(registerUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRegisterData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                register_status.text = "Đăng ký thành công!";
                HandleResponse(www.responseCode.ToString(), www.downloadHandler.text);
                loggedInEmail = register_emailInput.value;
                Debug.Log("Logged in: " + loggedInEmail);
                yield break;
            }
            else
            {
                register_status.text = HandleResponse(www.responseCode.ToString(), www.downloadHandler.text);
                loggedInEmail = "";
                yield break;
            }
        }
    }

    // --- PHẦN LOGGED IN (GIỮ NGUYÊN) ---
    private IEnumerator OpenLoggedInHub()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (string.IsNullOrEmpty(loggedInEmail)) yield break;

        opening.SetActive(false);
        registerHub.SetActive(false);
        loginHub.SetActive(false);
        loggedInHub.SetActive(true);
        StartCoroutine(SetUpLoggedInHub());
    }
    private IEnumerator SetUpLoggedInHub()
    {
        yield return null;
        var loggedInRoot = loggedInUI.rootVisualElement;

        greeting = loggedInRoot.Q<Label>("lblGreeting");
        leaderBoardBtn = loggedInRoot.Q<Button>("btnLeaderboard");
        shopBtn = loggedInRoot.Q<Button>("btnShop");
        logOutBtn = loggedInRoot.Q<Button>("btnLogout");

        greeting.text = $"Hello, {loggedInName}!";
        Debug.Log("Token: " + token);

        leaderBoardBtn.clicked -= OpenLeaderBoardHub;
        leaderBoardBtn.clicked += OpenLeaderBoardHub;

        shopBtn.clicked -= OpenShopHub;
        shopBtn.clicked += OpenShopHub;

        logOutBtn.clicked -= () => { };
        logOutBtn.clicked += () =>
        {
            Debug.Log("User logged out: " + loggedInEmail);
            OpenOpening();
        };
    }

    // --- PHẦN LEADERBOARD (GIỮ NGUYÊN) ---
    private void OpenLeaderBoardHub()
    {
        opening.SetActive(false);
        registerHub.SetActive(false);
        loginHub.SetActive(false);
        loggedInHub.SetActive(false);
        leaderBoardHub.SetActive(true);
        StartCoroutine(SetUpLeaderBoardHub());
    }
    private IEnumerator SetUpLeaderBoardHub()
    {
        yield return null;
        var leaderBoardRoot = leaderBoardUI.rootVisualElement;

        ldbCloseBtn = leaderBoardRoot.Q<Button>("btnClose");
        ldbCloseBtn.clicked -= CloseLeaderBoard;
        ldbCloseBtn.clicked += CloseLeaderBoard;

        scrollView = leaderBoardRoot.Q<ScrollView>("ScrollList");
        StartCoroutine(GetLeaderboardData());
    }
    private void CloseLeaderBoard()
    {
        leaderBoardHub.SetActive(false);
        loggedInHub.SetActive(true);
        StartCoroutine(SetUpLoggedInHub());
    }

    private IEnumerator GetLeaderboardData()
    {
        UnityWebRequest request = UnityWebRequest.Get(getAllPlayerUrl);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Accept", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ParseAndShowData(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Lỗi tải Leaderboard: " + request.error);
        }
    }
    private void ParseAndShowData(string jsonResponse)
    {
        try
        {
            ResponsePlayerListModel response = JsonUtility.FromJson<ResponsePlayerListModel>(jsonResponse);

            if (response != null && response.isSuccess && response.data != null)
            {
                RenderLeaderBoard(response.data);
            }
            else
            {
                Debug.LogWarning("Không có dữ liệu hoặc success = false");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Lỗi parse JSON: " + ex.Message);
        }
    }
    private void RenderLeaderBoard(List<PlayerModel> players)
    {
        scrollView.Clear();
        foreach (var player in players)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("row");
            row.AddToClassList("data-row");

            Label lblID = new Label(player.playerID.ToString());
            lblID.AddToClassList("col-id");

            Label lblName = new Label(player.characterName);
            lblName.AddToClassList("col-name");

            Label lblMode = new Label(player.gameModeName);
            lblMode.AddToClassList("col-mode");

            Label lblExp = new Label(player.experiencePoints.ToString());
            lblExp.AddToClassList("col-exp");

            row.Add(lblID);
            row.Add(lblName);
            row.Add(lblMode);
            row.Add(lblExp);
            scrollView.Add(row);
        }
    }


    // ========================================================================================
    // --- PHẦN SHOP HUB (ĐÃ ĐƯỢC CHỈNH SỬA CHO ĐÚNG LOGIC VÀ FIX LỖI) ---
    // ========================================================================================
    private void OpenShopHub()
    {
        opening.SetActive(false);
        registerHub.SetActive(false);
        loginHub.SetActive(false);
        loggedInHub.SetActive(false);
        leaderBoardHub.SetActive(false);
        shopHub.SetActive(true);
        StartCoroutine(SetUpShopHub());
    }

    private IEnumerator SetUpShopHub()
    {
        yield return null;
        var shopRoot = shopUI.rootVisualElement;

        // 1. Tìm Elements
        walletText = shopRoot.Q<Label>("lbl-wallet");
        shopCloseBtn = shopRoot.Q<Button>("btn-close");
        shopStatus = shopRoot.Q<Label>("lbl-status");
        ItemBtn = shopRoot.Q<Button>("tab-item");
        vehicleBtn = shopRoot.Q<Button>("tab-vehicle");
        buyBtn = shopRoot.Q<Button>("btn-buy");
        shopList = shopRoot.Q<ListView>("lst-products");

        // 2. Cấu hình ListView (Cách hiển thị dòng)
        ConfigureShopListLogic();

        walletText.text = $"Wallet: ${loggedInWalletBalance}";
        shopStatus.text = "Chọn sản phẩm để mua!";

        // 3. Gán sự kiện (Có trừ sự kiện cũ -= để tránh lỗi cộng dồn)
        shopCloseBtn.clicked -= CloseShop;
        shopCloseBtn.clicked += CloseShop;

        ItemBtn.clicked -= OnTabItemClicked;
        ItemBtn.clicked += OnTabItemClicked;

        vehicleBtn.clicked -= OnTabVehicleClicked;
        vehicleBtn.clicked += OnTabVehicleClicked;

        buyBtn.clicked -= OnBuyClicked;
        buyBtn.clicked += OnBuyClicked;

        // 4. Lấy dữ liệu
        StartCoroutine(GetItemVehicleData());
    }

    // -- Các hàm xử lý sự kiện tách rời cho Shop --
    private void CloseShop()
    {
        shopHub.SetActive(false);
        loggedInHub.SetActive(true);
        StartCoroutine(SetUpLoggedInHub());
    }
    private void OnTabItemClicked() => ConfigureTab("Item");
    private void OnTabVehicleClicked() => ConfigureTab("Vehicle");
    private void OnBuyClicked()
    {
        // Gọi hàm tính toán và lấy giá trị
        var totalPrice = UpdateTotalPrice();

        if (totalPrice == 0)
        {
            shopStatus.text = "Giỏ hàng đang trống!";
            return;
        }

        // Kiểm tra số dư (Biến loggedInWalletBalance phải được lấy từ API Login hoặc GetPlayerInfo trước đó)
        if (loggedInWalletBalance < totalPrice)
        {
            shopStatus.text = $"Không đủ tiền! (Thiếu ${totalPrice - loggedInWalletBalance})";
            return;
        }

        // Bắt đầu quy trình mua
        StartCoroutine(PurchaseProgress(totalPrice));
    }
    private IEnumerator PurchaseProgress(int amount)
    {
        shopStatus.text = "Đang xử lý...";

        // 1. Lọc ra các vật phẩm đang chọn mua (quantity > 0)
        List<PurchaseDTO> buyList = new List<PurchaseDTO>();

        foreach (var product in _allProducts)
        {
            if (product.quantity > 0)
            {
                buyList.Add(new PurchaseDTO
                {
                    playerID = loggedInID,
                    itemID = product.itemID,     // int? (tự động null nếu là vehicle)
                    vehicleID = product.vehicleID, // int? (tự động null nếu là item)
                    quantity = product.quantity
                });
            }
        }

        // 2. Tạo gói dữ liệu JSON
        PurchaseRequestDTO requestData = new PurchaseRequestDTO
        {
            totalCost = amount,
            items = buyList
        };

        string jsonBody = JsonUtility.ToJson(requestData);

        // 3. Gửi Request POST
        using (UnityWebRequest request = new UnityWebRequest(purchaseUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Header quan trọng
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + token); // Token xác thực người mua

            yield return request.SendWebRequest();

            // 4. Xử lý kết quả
            if (request.result == UnityWebRequest.Result.Success)
            {
                // A. Trừ tiền ở Client (để hiển thị ngay lập tức)
                loggedInWalletBalance -= amount;
                walletText.text = $"Wallet: ${loggedInWalletBalance:N0}";

                // B. Reset giỏ hàng
                foreach (var item in _allProducts)
                {
                    item.quantity = 0;
                }

                // C. Cập nhật lại giao diện ListView (để số lượng về 0)
                shopList.Rebuild();

                // D. Cập nhật lại dòng Status Total về 0
                UpdateTotalPrice();

                shopStatus.text = "Mua hàng thành công!";
            }
            else
            {
                // --- THẤT BẠI ---
                shopStatus.text = "Lỗi giao dịch: " + request.error;
                Debug.LogError("Purchase Error: " + request.downloadHandler.text);
            }
        }
    }

    // -- Cấu hình hiển thị ListView (Fix lỗi tăng số lượng bị nhân lên) --
    private void ConfigureShopListLogic()
    {
        UpdateTotalPrice();
        shopList.fixedItemHeight = 60; // Chiều cao dòng
        shopList.makeItem = () => shopItemTemplate.Instantiate();

        shopList.bindItem = (visualElement, index) =>
        {
            if (index >= _currentList.Count) return;
            var itemData = _currentList[index];

            var icon = visualElement.Q<VisualElement>("img-item-icon");
            var lblName = visualElement.Q<Label>("lbl-item-name");
            var lblPrice = visualElement.Q<Label>("lbl-item-price");
            var lblQty = visualElement.Q<Label>("lbl-quantity");
            var btnInc = visualElement.Q<Button>("btn-increase");
            var btnDec = visualElement.Q<Button>("btn-decrease");

            // Hiển thị tên (ưu tiên itemName -> vehicleName)
            string displayName = !string.IsNullOrEmpty(itemData.itemName) ? itemData.itemName : itemData.vehicleName;
            if (string.IsNullOrEmpty(displayName)) displayName = "Unknown Product";

            lblName.text = displayName;
            lblPrice.text = "$" + itemData.price;
            lblQty.text = itemData.quantity.ToString();
            icon.style.backgroundImage = null;

            if (!string.IsNullOrEmpty(itemData.imageUrl))
            {
                // Lưu ý: Việc gọi Coroutine trong bindItem có thể gây performance issue nếu list quá dài.
                // Nhưng với bài tập này thì chấp nhận được.
                StartCoroutine(LoadImage(itemData.imageUrl, icon));
            }

            // --- QUAN TRỌNG: FIX LỖI EVENT STACKING TRONG LISTVIEW ---
            // Nút Tăng (+)
            if (btnInc.userData is Action oldInc) btnInc.clicked -= oldInc; // Xóa action cũ
            Action newInc = () =>
            {
                itemData.quantity++;
                lblQty.text = itemData.quantity.ToString();
                UpdateTotalPrice();
            };
            btnInc.clicked += newInc;
            btnInc.userData = newInc; // Lưu action mới vào userData

            // Nút Giảm (-)
            if (btnDec.userData is Action oldDec) btnDec.clicked -= oldDec;
            Action newDec = () =>
            {
                if (itemData.quantity > 0)
                {
                    itemData.quantity--;
                    lblQty.text = itemData.quantity.ToString();
                    UpdateTotalPrice();
                }
            };
            btnDec.clicked += newDec;
            btnDec.userData = newDec;
        };
    }
    private IEnumerator LoadImage(string url, VisualElement targetElement)
    {
        // Kiểm tra URL hợp lệ
        if (string.IsNullOrEmpty(url)) yield break;

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Lấy Texture từ request
                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                // Gán Texture vào background của VisualElement
                targetElement.style.backgroundImage = new StyleBackground(texture);
            }
            else
            {
                 Debug.LogWarning("Không tải được ảnh: " + url);
            }
        }
    }
    private int UpdateTotalPrice()
    {
        if (_allProducts == null) return 0;

        // Tính tổng: Giá * Số lượng
        // Dùng long để tránh tràn số nếu giá trị quá lớn
        int total = 0;

        foreach (var item in _allProducts)
        {
            if (item.quantity > 0)
            {
                total += item.price * item.quantity;
            }
        }

        // Cập nhật UI (Format N0 để có dấu phẩy: 1,000)
        shopStatus.text = $"Total: ${total:N0}";

        return total;
    }
    private IEnumerator GetItemVehicleData()
    {
        UnityWebRequest request = UnityWebRequest.Get(getItemVehicleUrl);
         request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResult = request.downloadHandler.text;
            ResponseItemVehicleListModel wrapper = JsonUtility.FromJson<ResponseItemVehicleListModel>(jsonResult);

            if (wrapper != null && wrapper.isSuccess && wrapper.data != null)
            {
                // Lưu vào biến toàn cục
                _allProducts = wrapper.data;
                // Mặc định hiển thị tab Item
                ConfigureTab("Item");
            }
            else
            {
                shopStatus.text = wrapper != null ? wrapper.notification : "Lỗi data null";
            }
        }
        else
        {
            shopStatus.text = "Lỗi HTTP: " + request.error;
        }
    }
    private void ConfigureTab(string tabName)
    {
        // Kiểm tra dữ liệu rỗng
        if (_allProducts == null || _allProducts.Count == 0)
        {
            shopStatus.text = "Chưa có dữ liệu.";
            return;
        }

        if (tabName == "Item")
        {
            // [SỬA ĐỔI QUAN TRỌNG]
            // Logic cũ: x.type == "Item" -> SAI vì JSON trả về "Food", "Weapon"...
            // Logic mới: Kiểm tra itemID khác null và lớn hơn 0
            _currentList = _allProducts
                            .Where(x => x.itemID != null && x.itemID > 0)
                            .ToList();

            // Cập nhật giao diện Tab
            ItemBtn.AddToClassList("active-tab");
            vehicleBtn.RemoveFromClassList("active-tab");
        }
        else if (tabName == "Vehicle")
        {
            // [SỬA ĐỔI QUAN TRỌNG]
            // Kiểm tra vehicleID khác null và lớn hơn 0
            _currentList = _allProducts
                            .Where(x => x.vehicleID != null && x.vehicleID > 0)
                            .ToList();

            // Cập nhật giao diện Tab
            vehicleBtn.AddToClassList("active-tab");
            ItemBtn.RemoveFromClassList("active-tab");
        }

        // Cập nhật ListView
        shopList.itemsSource = _currentList;
        shopList.Rebuild();

        // Cuộn lên đầu danh sách (dùng ScrollToItem thay vì scrollOffset để tránh lỗi version)
        if (_currentList.Count > 0)
        {
            shopList.ScrollToItem(0);
        }
    }

    // ========================================================================================
    // --- MODELS ---
    // ========================================================================================

    [System.Serializable]
    private class LoginModel
    {
        public string email;
        public string password;
        public LoginModel(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }
    [System.Serializable]
    private class RegisterModel
    {
        public string CharacterName;
        public string Email;
        public string Password;
        public int GameModeID;
        public RegisterModel(string characterName, string email, string password, int gameModeID)
        {
            CharacterName = characterName;
            Email = email;
            Password = password;
            GameModeID = gameModeID;
        }
    }

    [System.Serializable]
    private class ResponseModel
    {
        public bool isSuccess;
        public string notification;
        public string token;
        public string loggedInName;
        public int playerID;
        public int walletBalance;
        public object data;
    }
    [System.Serializable]
    private class PlayerModel
    {
        public int playerID;
        public string email;
        public string password;
        public string characterName;
        public string gameModeName;
        public int experiencePoints;
        public int walletBalance;
    }

    [System.Serializable]
    private class ResponsePlayerListModel
    {
        public bool isSuccess;
        public string notification;
        public string token;
        public List<PlayerModel> data;
    }

    [System.Serializable]
    private class ItemVehicleModel
    {
        public int itemID;
        public string itemName; // Đã bỏ ? để tránh lỗi JsonUtility
        public int vehicleID;
        public string vehicleName;
        public string imageUrl;
        public string type;
        public int price;

        // Thêm biến quantity để Client tự quản lý
        [System.NonSerialized] public int quantity = 0;
    }
    [System.Serializable]
    private class ResponseItemVehicleListModel
    {
        public bool isSuccess;
        public string notification;
        public string token;
        public List<ItemVehicleModel> data;
    }
    [System.Serializable]
    public class PurchaseDTO
    {
        public int playerID;
        public int itemID;
        public int vehicleID;
        public int quantity;
    }

    [System.Serializable]
    public class PurchaseRequestDTO
    {
        public int totalCost;
        public List<PurchaseDTO> items;
    }
}