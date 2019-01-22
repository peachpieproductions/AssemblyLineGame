using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class GameController : MonoBehaviour {

    #region Public Vars

    public bool debugMode;
    public static GameController inst;
	public int roomWidth;
	public int roomHeight;
    public Vector2 mouseWorldPos;
    public Vector2Int currentUnit;
    public List<BaseEntity> entities = new List<BaseEntity>();
    public int[,] entityGrid = new int[50,50];
    public List<Item> itemPool = new List<Item>();
    public ResearchData researching;
    public float researchProgress;
    public bool buildMode;

    [Header("UI")]
    public EntityData currentBuildEntity;
    public Transform currentBuildObject;
    public BaseEntity selectedEntity;
    public Package selectedPackage;
    public bool hoveringOverlay;
    public List<OverlayMenu> hoveringList = new List<OverlayMenu>();
    public ItemData selectedRecipe;
    public bool selectingItemData;
    public ItemData selectedItemData;
    public Log logPrefab;

    [Header("Player")]
    public int money = 250;
    public List<StorageSlot> inventory = new List<StorageSlot>();
    public int researchesCompleted;
    public Vector2 timeOfDay;
    public Vector3 date;
    public List<ItemData> recipeList = new List<ItemData>();
    public List<ItemContract> contractList = new List<ItemContract>();
    public List<Package> outboundPackages = new List<Package>();
    public List<ItemDelivery> incomingDeliveries = new List<ItemDelivery>();
    public List<Zone> inboundZones = new List<Zone>();

    [Header("Refs")]
    public List<EntityData> entityDatas = new List<EntityData>();
    public List<ItemData> itemDatas = new List<ItemData>();
    public List<ResearchData> researchDatas = new List<ResearchData>();
    public GameObject itemPrefab;
    public GameObject packagePrefab;
    public GameObject blueprintFloor;
    public GameObject dummyPlacementEntity;
    public Color[] gameColors;
    public List<string> clientNames = new List<string> ();

    [Header("UI Refs")]
    public OverlayMenu BuildModeMenu;
    public OverlayMenu computer;
    public OverlayMenu inventoryMenu;
    public OverlayMenu entityMenu;
    public OverlayMenu recipeListMenu;
    public OverlayMenu marketplaceMenu;
    public OverlayMenu contractsMenu;
    public OverlayMenu researchMenu;
    public OverlayMenu PackageInfoPopup;
    public OverlayMenu ItemInfoPopup;
    public OverlayMenu tooltipPopup;
    public OverlayMenu TopHud;
    public TextMeshProUGUI MenuButtonsText;
    public QuickReferences hudResearchBar;

    #endregion
    
    Camera cam;
    Vector2 camMoveVelocity;
    float camZoomAmount = 5;

    private void Awake() {
        inst = this;
        cam = Camera.main;
        for (var i = 0; i < 50; i++) {
            var go = Instantiate(itemPrefab);
            itemPool.Add(go.GetComponent<Item>());
        }
    }

    private void Start() {
        foreach(BaseEntity b in FindObjectsOfType<BaseEntity>()) {
            entities.Add(b);
            b.currentCoord = new Vector2Int(Mathf.FloorToInt(b.transform.position.x), Mathf.FloorToInt(b.transform.position.y));
        }
        UpdateEntityGrid();
        foreach (BaseEntity b in FindObjectsOfType<BaseEntity>()) b.SetNeighbors();

        StartCoroutine(EconomyCycle());
        StartCoroutine(TimeCycle());

        BuildModeMenu.gameObject.SetActive(false);
        computer.gameObject.SetActive(false);

        //Init Research
        foreach(ResearchData r in researchDatas) {
            foreach(ItemData i in r.items) {
                i.isUnlocked = false;
            }
            r.researched = false;
            r.beingResearched = false;
            r.GenerateCost();
            if (r.researchedOnStart) r.UnlockResearch();
        }

        //Recipe List
        recipeList.Clear();
        foreach (ItemData d in itemDatas) {
            if (d.recipe.Length > 0 && d.isUnlocked) {
                recipeList.Add(d);
            }
        }

        UpdateMoney(0);
        //LoadClientNames();

        //DEBUG MODE
        GameObject.Find("DEBUGMODEACTIVETEXT").gameObject.SetActive(debugMode);
        if (debugMode) {
            
        }
    }

    private void Update() {
        mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
        currentUnit = new Vector2Int (Mathf.Clamp(Mathf.FloorToInt(mouseWorldPos.x),0,50), Mathf.Clamp(Mathf.FloorToInt(mouseWorldPos.y),0,50));

        CameraMovement();

        if (Input.GetMouseButtonDown(0)) {
            if (!hoveringOverlay) {
                if (computer.open) {
                    computer.ToggleOpenClose(false);
                }
                if (selectedEntity) {
                    selectedEntity = null;
                    entityMenu.ToggleOpenClose(false);
                }
                if (selectedPackage) {
                    selectedPackage = null;
                    PackageInfoPopup.ToggleOpenClose(false);
                }
                if (ItemInfoPopup.gameObject.activeSelf) ItemInfoPopup.ToggleOpenClose(false);
            }
        }

        if (tooltipPopup.open) {
            if (!hoveringOverlay) {
                tooltipPopup.ToggleOpenClose(false);
            }
        }

        if (MenuButtonsText.gameObject.activeSelf) {
            if (Input.mousePosition.y > 80) MenuButtonsText.gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (computer.open) computer.ToggleOpenClose(false);
            else if (ItemInfoPopup.open) ItemInfoPopup.ToggleOpenClose(false);
            else if (recipeListMenu.open) recipeListMenu.ToggleOpenClose(false);
            else if (entityMenu.open) entityMenu.ToggleOpenClose(false);
            
            if (buildMode) ToggleBuildMode();
        }

        if (Input.GetKeyDown(KeyCode.Tab)) {
            computer.ToggleOpenClose(!computer.gameObject.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            ToggleBuildMode();
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            Time.timeScale = 4;
        }
        if (Input.GetKeyUp(KeyCode.F)) {
            Time.timeScale = 1;
        }

        
    }

    public IEnumerator TimeCycle() {
        TopHud.miscText3.text = date.x + "/" + date.y + "/" + date.z;
        while (true) {
            timeOfDay.y++; //next minute
            if (timeOfDay.y == 60) {
                timeOfDay.y = 1;
                timeOfDay.x++; //next hour
                if (timeOfDay.x == 24) {
                    timeOfDay.x = 0;
                    date.y++; //next day
                    TopHud.miscText3.text = date.x + "/" + date.y + "/" + date.z;
                    if (date.y == 32) {
                        date.y = 1;
                        date.x++; //next month
                        TopHud.miscText3.text = date.x + "/" + date.y + "/" + date.z;
                        if (date.x == 13) {
                            date.x = 1;
                            date.y++; //next year
                            TopHud.miscText3.text = date.x + "/" + date.y + "/" + date.z;
                        }
                    }
                }
            }
            if (timeOfDay.y < 10) TopHud.miscText2.text = timeOfDay.x + ":0" + timeOfDay.y;
            else TopHud.miscText2.text = timeOfDay.x + ":" + timeOfDay.y;
            
            yield return new WaitForSeconds(.5f);
        }
    }

    public IEnumerator BuildMode() {
        if (computer.open) computer.ToggleOpenClose(false);
        if (entityMenu.open) entityMenu.ToggleOpenClose(false);
        if (ItemInfoPopup.open) ItemInfoPopup.ToggleOpenClose(false);
        while (buildMode) {
            if (currentBuildObject == null) {
                if (currentBuildEntity) {
                    currentBuildObject = Instantiate(dummyPlacementEntity).transform;
                    currentBuildObject.GetComponent<SpriteRenderer>().sprite = currentBuildEntity.sprite;
                    if (currentBuildEntity.placementSprite) {
                        currentBuildObject.GetChild(0).gameObject.SetActive(true);
                        currentBuildObject.GetChild(0).GetComponent<SpriteRenderer>().sprite = currentBuildEntity.placementSprite;
                    }
                }
            } else {
                currentBuildObject.position = new Vector2(currentUnit.x + .5f, currentUnit.y + .5f); //Position Blueprint
                if (Input.GetKeyDown(KeyCode.R) && !currentBuildEntity.cantBeRotated) currentBuildObject.Rotate(0, 0, 90); //Rotate Blueprint
                var canPlace = true;
                for(var x = 0; x < currentBuildEntity.size.x; x++) { //Check if can be placed
                    for (var y = 0; y < currentBuildEntity.size.y; y++) {
                        if (entityGrid[Mathf.Min(currentUnit.x + x, 49), Mathf.Min(currentUnit.y + y, 49)] != 0 || currentUnit.x + x == 0 || currentUnit.x + x == 49 || currentUnit.y + y == 0 || currentUnit.y + y == 49) {
                            if (canPlace) currentBuildObject.GetComponent<SpriteRenderer>().color = gameColors[1];
                            canPlace = false;
                        }
                    }
                }
                if (canPlace) currentBuildObject.GetComponent<SpriteRenderer>().color = gameColors[0];
                if (Input.GetMouseButtonDown(0) && Input.mousePosition.x < Screen.width * .875f) { //Place Entity
                    if (canPlace && money >= currentBuildEntity.cost) { //Check if entity already exists at position
                        UpdateMoney(-currentBuildEntity.cost);
                        var newEntity = Instantiate(currentBuildEntity.prefab, new Vector2(currentUnit.x + .5f, currentUnit.y + .5f), currentBuildObject.rotation);
                        entities.Add(newEntity.GetComponent<BaseEntity>());
                        var entityID = entities.Count;
                        for (var x = 0; x < currentBuildEntity.size.x; x++) { //Set Grid Data
                            for (var y = 0; y < currentBuildEntity.size.y; y++) {
                                entityGrid[currentUnit.x + x, currentUnit.y + y] = entityID;
                            }
                        }
                        
                    }
                }
            }
            if (Input.GetMouseButtonDown(1)) { //Sell Entity
                if (entityGrid[currentUnit.x, currentUnit.y] > 0) {
                    var toSell = entities[entityGrid[currentUnit.x, currentUnit.y] - 1];
                    foreach(StorageSlot slot in toSell.storage) {
                        if (!AddToInventory(slot.data, slot.itemCount, slot)) {
                            for (var i = 0; i < slot.itemCount; i++) {
                                var spawnedItem = SpawnItem(slot.data).transform;
                                spawnedItem.transform.position = toSell.transform.position;
                            }
                        }
                    }
                    UpdateMoney(toSell.data.cost / 2);
                    for (var x = 0; x < toSell.size.x; x++) { //Reset Grid Data
                        for (var y = 0; y < toSell.size.y; y++) {
                            entityGrid[toSell.currentCoord.x + x, toSell.currentCoord.y + y] = 0;
                        }
                    }
                    toSell.UpdateNeighbors();
                    entities.Remove(toSell);
                    Destroy(toSell.gameObject);
                    UpdateEntityGrid();
                } else if (currentBuildEntity) {
                    if (currentBuildObject) Destroy(currentBuildObject.gameObject);
                    currentBuildObject = null;
                    currentBuildEntity = null;
                } else {
                    ToggleBuildMode();
                }
            }
            yield return null;
        }
        if (currentBuildObject) Destroy(currentBuildObject.gameObject);
        currentBuildObject = null;
    }

    public IEnumerator EconomyCycle() {

        foreach(ItemData item in itemDatas) {
            item.priceVariant = 1;
            item.popularity = .5f;
        }

        while (true) {

            for (var i = incomingDeliveries.Count - 1; i >= 0; i--) {
                var d = incomingDeliveries[i];
                d.timeRemaining -= 10f;
                if (d.timeRemaining <= 0) {
                    Delivery(d);
                }
            }

            foreach(ItemData data in itemDatas) {
                data.priceVariant = Mathf.Clamp(data.priceVariant + Random.Range(-.1f, .1f) * .25f, .5f, 2);
                if (data.priceVariant > 1.2f) data.priceVariant -= .015f;
                data.popularity = Mathf.Clamp(data.popularity + Random.Range(-.1f, .1f) * .25f, 0, 1);
            }
            if (marketplaceMenu.open) UpdateMarketplace();
            GenerateContracts();
            yield return new WaitForSeconds(10f);

        }

    }

    public void UpdateMoney(int differenceAmount) {
        money += differenceAmount;
        TopHud.miscText1.text = "$" + money.ToString("n0");
        if (differenceAmount != 0) {
            if (differenceAmount > 0) GenerateLog("+ $" + differenceAmount + ".", gameColors[4], 4f);
            else GenerateLog("- $" + Mathf.Abs(differenceAmount) + ".", gameColors[3], 4f);
        }
    }

    public void Delivery(ItemDelivery delivery) {
        var zone = inboundZones[Random.Range(0, inboundZones.Count)];
        Vector2 placement = new Vector2(Random.Range(0f, 2f), Random.Range(0f, 2f));
        var itemCount = delivery.itemCount;
        while (itemCount > 0) {
            var newPackage = Instantiate(packagePrefab, (Vector2)zone.transform.position + placement,Quaternion.identity).GetComponent<Package>();
            newPackage.storage.data = delivery.data;
            newPackage.spriteRenderer.sprite = delivery.data.sprite;
            newPackage.storage.itemCount = Mathf.Min(10, itemCount);
            newPackage.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            itemCount -= 10;
        }
        GenerateLog("You're shipment of " + delivery.data.name + " (" + delivery.itemCount + ") has arrived.", Color.white, 4f);
        incomingDeliveries.Remove(delivery);
    }

    /*public void LoadClientNames() {
        clientNames.Clear();
        readTextFile(Application.dataPath + "/Data/Names.txt");
        //clientNames = new List<string>
    }
    void readTextFile(string file_path) {
        StreamReader inp_stm = new StreamReader(file_path);

        while (!inp_stm.EndOfStream) {
            string inp_ln = inp_stm.ReadLine();
            var lastIndex = inp_ln.IndexOf(" ", inp_ln.IndexOf(" ") + 1);
            if (lastIndex > 0) {
                inp_ln = inp_ln.Remove(lastIndex);
            }
            clientNames.Add(inp_ln);
        }

        inp_stm.Close();
    }*/

    public void RefreshOverlays() {
        foreach(OverlayMenu menu in FindObjectsOfType<OverlayMenu>()) {
            if (menu.menuName == "Marketplace") continue;
            menu.BuildMenu();
        }
    }

    public void ToggleBuildMode() {
        buildMode = !buildMode;
        if (buildMode) GenerateLog("Build Mode Activated.", Color.white, 1f);
        else GenerateLog("Build Mode Deactivated.", Color.white, 1f);
        blueprintFloor.SetActive(buildMode);
        BuildModeMenu.ToggleOpenClose(buildMode);
        if (buildMode) StartCoroutine(BuildMode());
        else currentBuildEntity = null;
    }

    public void UpdateEntityGrid() {
        int i = 1;
        foreach(BaseEntity e in entities) {
            for (var x = 0; x < e.size.x; x++) { //Set Grid Data
                for (var y = 0; y < e.size.y; y++) {
                    entityGrid[e.currentCoord.x + x, e.currentCoord.y + y] = i;
                }
            }
            i++;
        }
    }

    public void CameraMovement() {

        //Camera Zoom
        float camMoveZoomMult = 0;
        if (Input.mouseScrollDelta.y != 0) {
            if (!hoveringOverlay || (hoveringList.Count > 0 && !hoveringList[0].blockScrolling)) {
                camZoomAmount -= Input.mouseScrollDelta.y;
                camMoveZoomMult = (1f / cam.orthographicSize * Input.mouseScrollDelta.y);
                camZoomAmount = Mathf.Clamp(camZoomAmount, 1f, 14f);
            }
        }
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, camZoomAmount, Time.deltaTime * 10f);

        //Drag Camera
        if (Input.GetMouseButton(1)) camMoveVelocity -= new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * Time.unscaledDeltaTime * (cam.orthographicSize - 1.5f);

        //Camera Movement and Bounds
        camMoveVelocity += new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * Time.unscaledDeltaTime;
        camMoveVelocity *= .9f;
        camMoveVelocity += (mouseWorldPos - (Vector2)cam.transform.position) * camMoveZoomMult * .1f; //zoom towards mouse cursor
        cam.transform.position += (Vector3)camMoveVelocity;
        cam.transform.position = new Vector3(Mathf.Clamp(cam.transform.position.x, cam.orthographicSize * cam.aspect, 50 - cam.orthographicSize * cam.aspect),
            Mathf.Clamp(cam.transform.position.y, cam.orthographicSize, 50 - cam.orthographicSize), cam.transform.position.z);

    }

    public bool AddToInventory(ItemData data, int amount, StorageSlot fromSlot = null) {
        bool addedToInv = false;
        foreach(StorageSlot slot in inventory) { //find existing slot
            if (slot.data == data) {
                slot.itemCount += amount;
                addedToInv = true;
                break;
            }
        }
        if (!addedToInv) {
            foreach (StorageSlot slot in inventory) { //find new slot
                if (slot.itemCount == 0) {
                    slot.data = data;
                    slot.itemCount += amount;
                    addedToInv = true;
                    break;
                }
            }
        }
        if (addedToInv && fromSlot != null) {
            fromSlot.itemCount -= amount;
            if (fromSlot.itemCount == 0) fromSlot.data = null;
        }
        if (addedToInv) inventoryMenu.BuildMenu();
        return addedToInv;
    }

    public Item SpawnItem(ItemData data) {
        if (itemPool.Count == 0) {
            var go = Instantiate(itemPrefab);
            itemPool.Add(go.GetComponent<Item>());
        }
        var newItem = itemPool[0];
        itemPool.RemoveAt(0);
        newItem.gameObject.SetActive(true);
        newItem.Set(data);
        return newItem;
    }

    public void DespawnItem(Item item) {
        foreach (BaseEntity entity in item.inEntityZone) { if (entity) entity.itemsInZone.Remove(item.transform); }
        itemPool.Add(item);
        item.gameObject.SetActive(false);
    }

    public void AddToMarketplaceCart(UIButton listing) {
        if (listing.itemData) {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1)) listing.storageSlot.itemCount += 10;
            else listing.storageSlot.itemCount++;
        }
        UpdateMarketplace();
    }

    public void RemoveFromMarketplaceCart(UIButton listing) {
        if (listing.itemData) {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1)) listing.storageSlot.itemCount -= 10;
            else listing.storageSlot.itemCount--;
            if (listing.storageSlot.itemCount < 0) listing.storageSlot.itemCount = 0;
        }
        UpdateMarketplace();
    }

    public void UpdateMarketplace() {
        int cartTotal = 0;
        foreach(UIButton listing in marketplaceMenu.buttons) {
            listing.miscText2.text = listing.storageSlot.itemCount.ToString();
            listing.miscText1.text = "$" + listing.itemData.getCurrentPrice().ToString();
            cartTotal += listing.storageSlot.itemCount * listing.itemData.getCurrentPrice();
            if (listing.itemData.getCurrentPrice() > listing.itemData.basePrice) listing.miscImages[0].gameObject.SetActive(true); 
            else listing.miscImages[0].gameObject.SetActive(false);
            if (listing.itemData.getCurrentPrice() < listing.itemData.basePrice) listing.miscImages[1].gameObject.SetActive(true);
            else listing.miscImages[1].gameObject.SetActive(false);
        }
        marketplaceMenu.miscText1.GetComponent<TextMeshProUGUI>().text = "$" + cartTotal;
    }

    public void MarketplaceCheckout() {
        int cartTotal = 0;
        foreach (UIButton listing in marketplaceMenu.buttons) {
            cartTotal += listing.storageSlot.itemCount * listing.itemData.getCurrentPrice();
        }
        if (cartTotal > 0 && money >= cartTotal) {
            UpdateMoney(-cartTotal);
            foreach (UIButton listing in marketplaceMenu.buttons) {
                if (listing.storageSlot.itemCount > 0) {
                    ItemDelivery del = new ItemDelivery();
                    del.data = listing.itemData;
                    del.itemCount = listing.storageSlot.itemCount;
                    incomingDeliveries.Add(del);
                    listing.storageSlot.itemCount = 0;
                }
            }
            UpdateMarketplace();
            RefreshOverlays();
        }
        
    }

    public void GenerateContracts() {

        for (int i = contractList.Count - 1; i >= 0; i--) {
            contractList[i].hoursTimeRemaining -= .166f * 2f;
            if (contractList[i].hoursTimeRemaining <= 0) {
                contractList.Remove(contractList[i]);
            }
        }

        if (contractList.Count < 8 + researchesCompleted && Random.value > .4f && recipeList.Count > 0) {
            for (var j = 0; j < Random.Range(0, 3); j++) {
                ItemContract newContract = new ItemContract();
                newContract.clientName = GetClientName();
                for (var i = 0; i < Random.Range(1, 2); i++) {
                    StorageSlot itemRequirement = new StorageSlot();
                    do { itemRequirement.data = recipeList[Random.Range(0, recipeList.Count)]; } while (!itemRequirement.data.isProduct);
                    itemRequirement.itemCount = Random.Range(5, 20);
                    newContract.paymentAmount += itemRequirement.data.basePrice * itemRequirement.itemCount;
                    newContract.itemsRequested.Add(itemRequirement);
                }
                newContract.hoursTimeRemaining = Random.Range(4, 45);
                newContract.paymentAmount = Mathf.RoundToInt(newContract.paymentAmount * (1 + Random.Range(.1f, .3f)));
                contractList.Add(newContract);
            }
        }

        CheckForCompletedContracts();

    }

    public void CompleteContract(UIButton contractListing) {
        var contract = contractList[contractListing.transform.GetSiblingIndex() - 1];
        foreach (StorageSlot req in contract.itemsRequested) {
            int totalToRemove = req.itemCount;
            foreach (Package p in outboundPackages) {
                if (p.storage.data == req.data) {
                    var amountGiven = Mathf.Min(p.storage.itemCount, totalToRemove);
                    p.storage.itemCount -= amountGiven;
                    totalToRemove -= amountGiven;
                    if (p.storage.itemCount == 0) Destroy(p.gameObject);
                }
            }
        }
        UpdateMoney(contract.paymentAmount);
        contractList.RemoveAt(contractListing.transform.GetSiblingIndex()-1);
        contractsMenu.buttons.Remove(contractListing);
        Destroy(contractListing.gameObject);
        CheckForCompletedContracts();
        inventoryMenu.BuildMenu();
    }

    public string GetClientName() {
        return clientNames[Random.Range(0, clientNames.Count)];
    }

    public void CheckForCompletedContracts() {
        //Check if completed
        int totalComplete = 0;
        foreach (ItemContract c in contractList) {
            bool contractComplete = true;
            foreach (StorageSlot req in c.itemsRequested) {
                bool itemSetComplete = false;
                int totalItemCount = 0;
                foreach (Package p in outboundPackages) {
                    if (p.storage.data == req.data) {
                        totalItemCount += p.storage.itemCount;
                        if (totalItemCount >= req.itemCount) {
                            itemSetComplete = true;
                            break;
                        }
                    }
                }
                if (contractComplete) contractComplete = itemSetComplete;
            }
            c.completed = contractComplete;
            if (contractComplete) totalComplete++;
        }
        if (totalComplete >= 2 && Random.value < .25f) GenerateLog("2+ Contracts are ready to be fulfilled.", gameColors[4], 2f);

        if (contractsMenu.open) contractsMenu.BuildMenu();
    }

    public void StartResearch(ResearchData data) {
        if (!data.researched && researching == null && money >= data.cost) {
            UpdateMoney(-data.cost);
            GenerateLog("You've started researching " + data.name + ".", gameColors[2], 5f);
            researching = data;
            researching.beingResearched = true;
            researchProgress = 0;
            hudResearchBar.gameObject.SetActive(true);
            hudResearchBar.texts[0].text = "Researching " + data.name;
            StartCoroutine(Research());
        }
    }

    public IEnumerator Research() {
        while(researching) {
            researchProgress += 1f;
            hudResearchBar.rects[0].localScale = new Vector3(researchProgress / researching.cost, 1, 1);
            if (researchMenu.gameObject.activeSelf) researchMenu.BuildMenu();
            if (researchProgress >= researching.cost) {
                researching.UnlockResearch();
                hudResearchBar.gameObject.SetActive(false);
                GenerateLog("You've finished researching " + researching.name + ".", gameColors[2], 8f);
                researching = null;
                if (researchMenu.gameObject.activeSelf) researchMenu.BuildMenu();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void OpenItemDataInfo(ItemData itemData) {
        selectedItemData = itemData;
        ItemInfoPopup.ToggleOpenClose(true);
        ItemInfoPopup.BuildMenu();
    }

    public void GenerateLog(string log, Color col, float life) {
        var newLog = Instantiate(logPrefab, logPrefab.transform.parent);
        newLog.gameObject.SetActive(true);
        newLog.text.text = log;
        newLog.text.ForceMeshUpdate();
        newLog.text.color = col;
        newLog.life = life;
        newLog.rt.sizeDelta = new Vector2(40 + newLog.text.textBounds.extents.x * 2, 50);
    }

    public void SetMenuButtonsText(string str) {
        MenuButtonsText.gameObject.SetActive(true);
        MenuButtonsText.text = str;
    }

    public void DebugCommand(string command) {
        if (debugMode) {
            if (command == "GiveMoney") {
                UpdateMoney(1000);
            }
            if (command == "ResearchFaster") {
                researchProgress += 50;
            }
        }
    }

}
