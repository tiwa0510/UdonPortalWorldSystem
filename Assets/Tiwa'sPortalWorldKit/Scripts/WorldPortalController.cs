using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class WorldPortalController : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)]
    public string syncWorldID;
    string localSelectID;

    public Text SyncButtonText;

    public VRC.SDK3.Components.VRCPortalMarker VRCPortalMarker;
    public TextAsset WorldInfoTextAsset;
    public TextAsset WorldTagsTextAsset;
    public TextAsset WorldAuthorListTextAsset;
    public TextAsset WorldFilterCapacityTextAsset;
    public TextAsset WorldFilterSizeTextAsset;
    public TextAsset WorldFilterPublicTextAsset;

    string[] TagTerm;
    string[] AuthorTerm;
    int[] CapacityTerm;
    float[] SizeTerm;

    // WorldInfo
    string[] worldName;
    string[] worldAuthor;
    int[] worldCapacity;
    float[] worldSize;
    string[] worldID;
    string[] worldDescrption;
    bool[] worldIsPublic;
    string[][] worldTags;

    int worldCount;
    int tagCount;
    int authorCount;

    // World thumbnail resourcesa
    public Transform ImageBankRoot;
    public Image dummyImage;
    Image[] ImageBankResources;
    string[] ImageBankResourcesID;

    // World List
    public Transform WorldListRoot;
    string[] worldListID;
    Button[] worldListButton;
    Image[] worldListThumbnail;
    Text[] worldListlTitle;
    int WORLDLIST_COUNT_MAX;

    // Filter UI
    public Toggle FilterWorldCapacityIsGraterToggle;
    public Toggle FilterWorldSizeIsGraterToggle;
    public Toggle FilterWorldUnlistOnly;
    public Toggle FilterWorldPublicOnly;
    public Toggle FilterWorldTagsToggle;

    public Text FilterCapacityStateText;
    public Text FilterSizeStateText;

    public Text FilterWorldAuthorText;
    public Text FilterWorldCapacityText;
    public Text FilterWorldSizeText;

    // Filter Param
    string selectAuthor = "";
    int selectCapacity = 0;
    float selectSize = 0;
    string[] selectTags;

    // Author List
    public Transform worldAuthorListRoot;
    Button[] worldAuthorListButton;
    int WORLD_AUTHOR_LIST_COUNT_MAX;

    // Filter Capacity List
    public Transform worldFilterCapacityListRoot;
    Button[] worldFilterCapacityListButton;
    int WORLD_CAPACITY_LIST_COUNT_MAX;

    // Filter Size List
    public Transform worldFilterSizeListRoot;
    Button[] worldFilterSizeListButton;
    int WORLD_SIZE_LIST_COUNT_MAX;


    // Tag List
    public Transform worldTagListRoot;
    Button[] worldTagListButton;
    int selectTagCount;
    public Text SelectTagDisplayText;
    int WORLD_TAG_LIST_COUNT_MAX;

    // Page Move
    public Text PageCountText;
    int pageCount = 1;
    int PAGE_COUNT_MAX;

    // Page Move
    public Text TagPageCountText;
    int tagPageCount = 1;
    int TAG_PAGE_COUNT_MAX;

    // Page Move
    public Text AuthorPageCountText;
    int authorPageCount = 1;
    int AUTHOR_PAGE_COUNT_MAX;

    // Display Info
    public Image DisplayThumbnail;
    public Text DisplayTitle;
    public Text DisplayAuthor;
    public Text DisplayDescription;
    public Text DisplayCapacity;
    public Text DisplaySize;
    public Text DisplayTags;
    public Text DisplayPublic;

    // SE
    public AudioSource audioSource;
    public AudioClip audioClip;

    public void Start()
    {
        #region Initalize World Info
        WORLDLIST_COUNT_MAX = WorldListRoot.childCount * WorldListRoot.GetChild(0).childCount;
        WORLD_CAPACITY_LIST_COUNT_MAX = worldFilterCapacityListRoot.childCount * worldFilterCapacityListRoot.GetChild(0).childCount;
        WORLD_SIZE_LIST_COUNT_MAX = worldFilterSizeListRoot.childCount * worldFilterSizeListRoot.GetChild(0).childCount; 
        WORLD_AUTHOR_LIST_COUNT_MAX = worldAuthorListRoot.childCount * worldAuthorListRoot.GetChild(0).childCount;
        WORLD_TAG_LIST_COUNT_MAX = worldTagListRoot.childCount * worldTagListRoot.GetChild(0).childCount;

        string[] worldInfoData = WorldInfoTextAsset.ToString().Split('\n');
        worldCount = worldInfoData.Length;
        PAGE_COUNT_MAX = worldCount / WORLDLIST_COUNT_MAX + 1;

        worldName = new string[worldInfoData.Length];
        worldAuthor = new string[worldInfoData.Length];
        worldCapacity = new int[worldInfoData.Length];
        worldSize = new float[worldInfoData.Length];
        worldID = new string[worldInfoData.Length];
        worldDescrption = new string[worldInfoData.Length];
        worldIsPublic = new bool[worldInfoData.Length];
        worldTags = new string[worldInfoData.Length][];
        #endregion

        #region Parse CSV
        for (int i = 0; i < worldInfoData.Length; i++)
        {
            string[] worldInfo = worldInfoData[i].Split(',');

            worldName[i] = worldInfo[0];
            worldAuthor[i] = worldInfo[1];
            worldCapacity[i] = int.Parse(worldInfo[2]);
            worldSize[i] = float.Parse(worldInfo[3]);
            worldID[i] = worldInfo[4];
            worldDescrption[i] = worldInfo[5].Replace("<br>", "\n");

            worldIsPublic[i] = worldInfo[6] == "Public" ? true : false;

            worldTags[i] = new string[7];
            for (int j = 0; 7 + j < worldInfo.Length; j++)
            {
                if (worldInfo[7 + j] == "") continue;
                worldTags[i][j] = worldInfo[7 + j];
            }
        }

        // Parse Other CSV
        string[] worldTagInfo = WorldTagsTextAsset.ToString().Split('\n');
        tagCount = worldTagInfo.Length;
        TAG_PAGE_COUNT_MAX = tagCount / WORLD_TAG_LIST_COUNT_MAX + 1;
        TagTerm = new string[tagCount];
        for(int i = 0; i < tagCount; i++)
        {
            string[] tagInfo = worldTagInfo[i].Split(',');
            TagTerm[i] = tagInfo[1].Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        }

        string[] worldAuthorInfo = WorldAuthorListTextAsset.ToString().Split('\n');
        authorCount = worldAuthorInfo.Length;
        AUTHOR_PAGE_COUNT_MAX = authorCount / WORLD_AUTHOR_LIST_COUNT_MAX + 1;
        AuthorTerm = new string[authorCount];
        for (int i = 0; i < authorCount; i++)
        {
            string[] authorInfo = worldAuthorInfo[i].Split(',');
            AuthorTerm[i] = authorInfo[1].Replace("\r\n", "").Replace("\r", "").Replace("\n", ""); ;
        }

        string[] worldCapacityInfo = WorldFilterCapacityTextAsset.ToString().Split('\n');
        CapacityTerm = new int[worldCapacityInfo.Length];
        for (int i = 0; i < worldCapacityInfo.Length; i++)
        {
            string[] capacityInfo = worldCapacityInfo[i].Split(',');
            CapacityTerm[i] = int.Parse(capacityInfo[1]);
        }

        string[] worldSizeInfo = WorldFilterSizeTextAsset.ToString().Split('\n');
        SizeTerm = new float[worldSizeInfo.Length];
        for (int i = 0; i < worldSizeInfo.Length; i++)
        {
            string[] sizeInfo = worldSizeInfo[i].Split(',');
            SizeTerm[i] = float.Parse(sizeInfo[1]);
        }

        #endregion

        // Initalize thumbnail List
        worldListButton = new Button[WORLDLIST_COUNT_MAX];
        worldListThumbnail = new Image[WORLDLIST_COUNT_MAX];
        worldListlTitle = new Text[WORLDLIST_COUNT_MAX];
        worldListID = new string[WORLDLIST_COUNT_MAX * PAGE_COUNT_MAX];
        for(int i = 0; i < WorldListRoot.childCount; i++)
        {
            Transform row = WorldListRoot.GetChild(i);
            for(int j = 0; j < row.childCount; j++)
            {
                int index = i * row.childCount + j;
                if (index >= WORLDLIST_COUNT_MAX) break;
                worldListButton[index] = row.GetChild(j).GetComponent<Button>();
                worldListThumbnail[index] = row.GetChild(j).GetComponent<Image>();
                worldListlTitle[index] = row.GetChild(j).GetComponentInChildren<Text>();
            }
        }

        // Initalize ImageBank
        ImageBankResources = new Image[ImageBankRoot.childCount];
        ImageBankResourcesID = new string[ImageBankRoot.childCount];
        for (int i = 0; i < ImageBankRoot.childCount; i++)
        {
            ImageBankResources[i] = ImageBankRoot.GetChild(i).GetComponent<Image>();
            ImageBankResourcesID[i] = ImageBankRoot.GetChild(i).name;
        }

        // Initalize Author List
        worldAuthorListButton = new Button[WORLD_AUTHOR_LIST_COUNT_MAX];
        for (int i = 0; i < worldAuthorListRoot.childCount; i++)
        {
            Transform row = worldAuthorListRoot.GetChild(i);
            for (int j = 0; j < row.childCount; j++)
            {
                int index = i * row.childCount + j;
                if (index >= WORLD_AUTHOR_LIST_COUNT_MAX) break;
                worldAuthorListButton[index] = row.GetChild(j).GetComponent<Button>();
                row.GetChild(j).GetComponentInChildren<Text>().text = AuthorTerm.Length > index ? AuthorTerm[index] : "---";
            }
        }

        // Initalize Capacity List
        worldFilterCapacityListButton = new Button[WORLD_CAPACITY_LIST_COUNT_MAX];
        for (int i = 0; i < worldFilterCapacityListRoot.GetChild(0).childCount; i++)
        {
            if (i >= WORLD_CAPACITY_LIST_COUNT_MAX) break;
            worldFilterCapacityListButton[i] = worldFilterCapacityListRoot.GetChild(0).GetChild(i).GetComponent<Button>();
            worldFilterCapacityListRoot.GetChild(0).GetChild(i).GetComponentInChildren<Text>().text = CapacityTerm.Length > i ? CapacityTerm[i].ToString() + "人" : "---";
        }

        // Initalize Size List
        worldFilterSizeListButton = new Button[WORLD_SIZE_LIST_COUNT_MAX];
        for (int i = 0; i < worldFilterSizeListRoot.GetChild(0).childCount; i++)
        {
            if (i >= WORLD_SIZE_LIST_COUNT_MAX) break;
            worldFilterSizeListButton[i] = worldFilterSizeListRoot.GetChild(0).GetChild(i).GetComponent<Button>();

            float size = SizeTerm.Length > i ? SizeTerm[i] : 0;
            string term = "";
            if(size < 1)
            {
                term = (size * 1000f).ToString() + "KB";
            }
            else if(size >= 1000)
            {
                term = (size / 1000f).ToString() + "GB";
            }
            else if(size == 0)
            {
                term = "---";
            }
            else
            {
                term = size.ToString() + "MB";
            }

            worldFilterSizeListRoot.GetChild(0).GetChild(i).GetComponentInChildren<Text>().text = term;
        }

        // Initalize TagList
        selectTags = new string[WORLD_TAG_LIST_COUNT_MAX];
        worldTagListButton = new Button[WORLD_TAG_LIST_COUNT_MAX];
        for (int i = 0; i < worldTagListRoot.childCount; i++)
        {
            Transform row = worldTagListRoot.GetChild(i);
            for (int j = 0; j < row.childCount; j++)
            {
                int index = i * row.childCount + j;
                if (index >= WORLD_TAG_LIST_COUNT_MAX) break;
                worldTagListButton[index] = row.GetChild(j).GetComponent<Button>();
                row.GetChild(j).GetComponentInChildren<Text>().text =  TagTerm.Length > index ? TagTerm[index] : "---";
            }
        }
        SelectTagDisplayText.text = "Tags : ";

        // First Update
        PageCountText.text = pageCount.ToString() + "/" + PAGE_COUNT_MAX.ToString();
        TagPageCountText.text = tagPageCount.ToString() + "/" + TAG_PAGE_COUNT_MAX.ToString();
        AuthorPageCountText.text = authorPageCount.ToString() + "/" + AUTHOR_PAGE_COUNT_MAX.ToString();
        UpdateWorldList();
    }

    public void UpdateWorldList()
    {
        // Reset World List
        DisplayThumbnail.sprite = null;
        for (int i = 0; i < WORLDLIST_COUNT_MAX * PAGE_COUNT_MAX; i++)
        {
            worldListID[i] = "";
        }

        for (int i = 0; i < WORLDLIST_COUNT_MAX; i++)
        {
            worldListlTitle[i].text = "";
            worldListThumbnail[i].sprite = dummyImage.sprite;
        }

        int hitCount = 0;
        for (int i = 0; i < worldCount; i++)
        {
            #region Filter 

            // 作者
            if (selectAuthor != "")
            {
                if (worldAuthor[i] != selectAuthor) continue;
            }

            // 人数
            if (selectCapacity > 0)
            {
                if (FilterWorldCapacityIsGraterToggle.isOn)
                {
                    if (worldCapacity[i] < selectCapacity) continue;
                }
                else
                {
                    if (worldCapacity[i] > selectCapacity) continue;
                }
            }

            // ワールドサイズ
            if (selectSize > 0)
            {
                if (FilterWorldSizeIsGraterToggle.isOn)
                {
                    if (worldSize[i] < selectSize) continue;
                }
                else
                {
                    if (worldSize[i] > selectSize) continue;
                }
            }

            // 公開されているかどうか
            if(FilterWorldUnlistOnly.isOn && !FilterWorldPublicOnly.isOn)
            {
                if (worldIsPublic[i]) continue;
            }

            if(FilterWorldPublicOnly.isOn && !FilterWorldUnlistOnly.isOn)
            {
                if (!worldIsPublic[i]) continue;
            }

            // タグ
            if (selectTags != null && selectTagCount > 0)
            {
                bool isMatch = false;

                // 部分一致
                if (!FilterWorldTagsToggle.isOn)
                {
                    for (int j = 0; j < selectTagCount; j++)
                    {
                        for (int k = 0; k < worldTags[i].Length; k++)
                        {
                            if (worldTags[i][k] == selectTags[j])
                            {
                                isMatch = true;
                                break;
                            }
                        }
                        if (isMatch) break;
                    }
                }
                // 完全一致
                else
                {
                    for (int j = 0; j < selectTagCount; j++)
                    {
                        for (int k = 0; k < worldTags[i].Length; k++)
                        {
                            if (worldTags[i][k] == selectTags[j])
                            {
                                isMatch = true;
                                break;
                            }
                            isMatch = false;
                        }
                        if (!isMatch) break;
                    }
                }
                if (!isMatch) continue;
            }
            #endregion

            worldListID[hitCount] = worldID[i];
            hitCount++;
            if (hitCount >= WORLDLIST_COUNT_MAX * PAGE_COUNT_MAX) break;
        }

        // Update World List
        for (int i = 0; i < WORLDLIST_COUNT_MAX; i++)
        {
            // Get World Title
            for (int j = 0; j < worldCount; j++)
            {
                if (worldID[j] == worldListID[i + WORLDLIST_COUNT_MAX * (pageCount - 1)])
                {
                    worldListlTitle[i].text = worldName[j];
                    break;
                }
            }

            // Get World Thumbnail
            for (int j = 0; j < ImageBankRoot.childCount; j++)
            {
                if (ImageBankResourcesID[j] == worldListID[i + WORLDLIST_COUNT_MAX * (pageCount - 1)])
                {
                    worldListThumbnail[i].sprite = ImageBankResources[j].sprite;
                    break;
                }
            }
        }
    }

    float setOwnerTimer = -1;
    float setIDtimer = -1;
    public void OnPressedSyncIDButton()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        setOwnerTimer = 1.5f;
        SyncButtonText.text = "同期中 ";
    }

    private void Update()
    {
        if (setOwnerTimer != -1)
        {
            setOwnerTimer -= Time.deltaTime;
            if(setOwnerTimer < 0.75f)
            {
                SyncButtonText.text = "同期中 .";
            }

            if (setOwnerTimer < 0)
            {
                syncWorldID = localSelectID;
                setOwnerTimer = -1;
                setIDtimer = 1.5f;
                SyncButtonText.text = "同期中 ..";
            }
        }

        if (setIDtimer != -1)
        {
            setIDtimer -= Time.deltaTime;
            if (setIDtimer < 0.75f)
            {
                SyncButtonText.text = "同期中 ...";
            }
            if (setIDtimer < 0)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncUpdateID");
                setIDtimer = -1;
                SyncButtonText.text = "Drop Portal";
            }
        }
    }

    public void SyncUpdateID()
    {
        UpdateWorldInfo(syncWorldID);
        VRCPortalMarker.roomId = syncWorldID;
    }

    // UI Action
    public void OnPressedWorldListButton()
    {
        for(int i = 0; i < WORLDLIST_COUNT_MAX; i++)
        {
            if (worldListButton[i] == null) continue;
            if (worldListButton[i].interactable) continue;
            audioSource.PlayOneShot(audioClip);
            worldListButton[i].interactable = true;

            string selectID = worldListID[i + WORLDLIST_COUNT_MAX * (pageCount - 1)];
            localSelectID = selectID;
            UpdateWorldInfo(selectID);
        }
    }

    public void UpdateWorldInfo(string selectID)
    {
        int index = -1;
        for (int j = 0; j < worldCount; j++)
        {
            if (worldID[j] == selectID)
            {
                index = j;
                break;
            }
        }
        if (index == -1) return;

        DisplayTitle.text = worldName[index];
        DisplayAuthor.text = worldAuthor[index];
        DisplayCapacity.text = worldCapacity[index] + "人";
        DisplayDescription.fontSize = worldDescrption[index].Length >= 88 ? 40 : 51;
        DisplayDescription.text = worldDescrption[index];

        DisplayPublic.text = worldIsPublic[index] ? "公開" : "非公開";

        float size = worldSize[index];
        string sizeText = "";
        if (size < 1)
        {
            sizeText = (size * 1000f).ToString() + "KB";
        }
        else if (size >= 1000)
        {
            sizeText = (size / 1000f).ToString() + "GB";
        }
        else if (size == 0)
        {
            sizeText = "---";
        }
        else
        {
            sizeText = size.ToString() + "MB";
        }
        DisplaySize.text = sizeText;

        DisplayTags.text = "";
        for (int j = 0; j < worldTags[index].Length; j++)
        {
            if (worldTags[index][j] == null) continue;
            if (worldTags[index][j] == "") continue;
            if (j > 0) DisplayTags.text += ", ";
            DisplayTags.text += worldTags[index][j];
        }

        // Get World Thumbnail
        for (int j = 0; j < ImageBankRoot.childCount; j++)
        {
            if (ImageBankResourcesID[j] == worldID[index])
            {
                DisplayThumbnail.sprite = ImageBankResources[j].sprite;
                break;
            }
        }
    }

    public void OnPressedPrevPageButton()
    {
        pageCount--;
        if(pageCount < 1)
        {
            pageCount = PAGE_COUNT_MAX;
        }
        PageCountText.text = pageCount.ToString() + "/" + PAGE_COUNT_MAX.ToString();
        UpdateWorldList();
        audioSource.PlayOneShot(audioClip);
    }

    public void OnPressedNextPageButton()
    {
        pageCount++;
        if(pageCount > PAGE_COUNT_MAX)
        {
            pageCount = 1;
        }
        PageCountText.text = pageCount.ToString() + "/" + PAGE_COUNT_MAX.ToString();
        UpdateWorldList();
        audioSource.PlayOneShot(audioClip);
    }

    public void OnPressedWorldTagButton()
    {
        for (int i = 0; i < WORLD_TAG_LIST_COUNT_MAX; i++)
        {
            if (worldTagListButton[i] == null) continue;
            if (worldTagListButton[i].interactable) continue;

            audioSource.PlayOneShot(audioClip);
            worldTagListButton[i].interactable = true;

            int index = -1;
            string tagName = worldTagListButton[i].GetComponentInChildren<Text>().text;
            for (int j = 0; j < selectTagCount; j++)
            {
                if (selectTags[j] == tagName)
                {
                    index = -2;
                    break;
                }
            }
            if (index == -2) continue;

            for (int j = 0; j < tagCount; j++)
            {
                if (TagTerm[j] == tagName)
                {
                    index = j;
                    break;
                }
            }
            if (index == -1) continue;

            if (selectTagCount >= WORLD_TAG_LIST_COUNT_MAX) break;
            selectTags[selectTagCount] = TagTerm[i + WORLD_TAG_LIST_COUNT_MAX * (tagPageCount - 1)];
            selectTagCount++;

            SelectTagDisplayText.text = "Tags :";
            for(int j = 0; j < selectTagCount; j++)
            {
                if (j > 0) SelectTagDisplayText.text += ", ";
                SelectTagDisplayText.text += selectTags[j];
            }
            UpdateWorldList();
        }
    }

    public void UpdateTagList()
    {
        for (int i = 0; i < WORLD_TAG_LIST_COUNT_MAX; i++)
        {
            int index = i + WORLD_TAG_LIST_COUNT_MAX * (tagPageCount - 1);
            if(index < TagTerm.Length)
            {
                worldTagListButton[i].GetComponentInChildren<Text>().text = TagTerm[i + WORLD_TAG_LIST_COUNT_MAX * (tagPageCount - 1)];
            }
            else
            {
                worldTagListButton[i].GetComponentInChildren<Text>().text = "---";
            }
        }
    }

    public void OnPressedTagNextPageButton()
    {
        tagPageCount++;
        if(tagPageCount > TAG_PAGE_COUNT_MAX)
        {
            tagPageCount = 1;
        }
        TagPageCountText.text = tagPageCount.ToString() + "/" + TAG_PAGE_COUNT_MAX.ToString();
        UpdateTagList();
        audioSource.PlayOneShot(audioClip);
    }

    public void OnPressedTagPrevPageButton()
    {
        tagPageCount--;
        if(tagPageCount < 1)
        {
            tagPageCount = TAG_PAGE_COUNT_MAX;
        }
        TagPageCountText.text = tagPageCount.ToString() + "/" + TAG_PAGE_COUNT_MAX.ToString();
        UpdateTagList();
        audioSource.PlayOneShot(audioClip);
    }

    public void OnPressedResetTagButton()
    {
        for(int i = 0; i < WORLD_TAG_LIST_COUNT_MAX; i++)
        {
            selectTags[i] = "";
        }
        SelectTagDisplayText.text = "Tags :";
        selectTagCount = 0;
        UpdateWorldList();
        audioSource.PlayOneShot(audioClip);
    }

    public void OnPressedAuthorButton()
    {
        Debug.Log(12);
        for (int i = 0; i < WORLD_AUTHOR_LIST_COUNT_MAX; i++)
        {
            if (worldAuthorListButton[i] == null) continue;
            if (worldAuthorListButton[i].interactable) continue;

            audioSource.PlayOneShot(audioClip);
            worldAuthorListButton[i].interactable = true;

            int index = -1;
            string authorName = worldAuthorListButton[i].GetComponentInChildren<Text>().text;

            for (int j = 0; j < authorCount; j++)
            {
                if (AuthorTerm[j] == authorName)
                {
                    index = j;
                    break;
                }
            }
            if (index == -1) continue;

            selectAuthor = AuthorTerm[i + WORLD_AUTHOR_LIST_COUNT_MAX * (authorPageCount - 1)];
            FilterWorldAuthorText.text = selectAuthor;

            UpdateWorldList();
        }
    }

    public void UpdateAuthorList()
    {
        for (int i = 0; i < WORLD_AUTHOR_LIST_COUNT_MAX; i++)
        {
            int index = i + WORLD_AUTHOR_LIST_COUNT_MAX * (authorPageCount - 1);
            if(index < AuthorTerm.Length)
            {
                worldAuthorListButton[i].GetComponentInChildren<Text>().text = AuthorTerm[index];
            }
            else
            {
                worldAuthorListButton[i].GetComponentInChildren<Text>().text = "---";
            }
        }
    }

    public void OnPressedAuthorNextPageButton()
    {
        authorPageCount++;
        if(authorPageCount > AUTHOR_PAGE_COUNT_MAX)
        {
            authorPageCount = 1;
        }
        AuthorPageCountText.text = authorPageCount.ToString() + "/" + AUTHOR_PAGE_COUNT_MAX.ToString();
        UpdateAuthorList();
        audioSource.PlayOneShot(audioClip);
    }

    public void OnPressedAuthorPrevPageButton()
    {
        authorPageCount--;
        if(authorPageCount < 1)
        {
            authorPageCount = AUTHOR_PAGE_COUNT_MAX;
        }
        AuthorPageCountText.text = authorPageCount.ToString() + "/" + AUTHOR_PAGE_COUNT_MAX.ToString();
        UpdateAuthorList();
        audioSource.PlayOneShot(audioClip);
    }

    public void OnPressedAuthorResetButton()
    {
        selectAuthor = "";
        FilterWorldAuthorText.text = "";
        UpdateWorldList();
        audioSource.PlayOneShot(audioClip);
    }

    public void OnPressedCapacityButton()
    {
        for (int i = 0; i < WORLD_CAPACITY_LIST_COUNT_MAX; i++)
        {
            if (worldFilterCapacityListButton[i] == null) continue;
            if (worldFilterCapacityListButton[i].interactable) continue;

            audioSource.PlayOneShot(audioClip);
            worldFilterCapacityListButton[i].interactable = true;

            selectCapacity = CapacityTerm[i];
            FilterWorldCapacityText.text = CapacityTerm[i].ToString() + "人";
            UpdateWorldList();
        }
    }

    public void OnPressedCapacityResetButton()
    {
        selectCapacity = 0;
        FilterWorldCapacityText.text = "";
        UpdateWorldList();
        audioSource.PlayOneShot(audioClip);
    }

    public void OnPressedSizeButton()
    {
        for (int i = 0; i < WORLD_SIZE_LIST_COUNT_MAX; i++)
        {
            if (worldFilterSizeListButton[i] == null) continue;
            if (worldFilterSizeListButton[i].interactable) continue;

            audioSource.PlayOneShot(audioClip);
            worldFilterSizeListButton[i].interactable = true;

            selectSize = SizeTerm[i];

            string term = "";
            if (selectSize < 1)
            {
                term = (selectSize * 1000f).ToString() + "KB";
            }
            else if (selectSize >= 1000)
            {
                term = (selectSize / 1000f).ToString() + "GB";
            }
            else if (selectSize == 0)
            {
                term = "---";
            }
            else
            {
                term = selectSize.ToString() + "MB";
            }

            FilterWorldSizeText.text = term;
            UpdateWorldList();
        }
    }

    public void OnPressedSizeResetButton()
    {
        selectSize = 0;
        FilterWorldSizeText.text = "";
        UpdateWorldList();
        audioSource.PlayOneShot(audioClip);
    }

    public void OnUpdateFilterStateText()
    {
        FilterCapacityStateText.text = FilterWorldCapacityIsGraterToggle.isOn ? "以上" : "以下";
        FilterSizeStateText.text = FilterWorldSizeIsGraterToggle.isOn ? "以上" : "以下";

        audioSource.PlayOneShot(audioClip);
    }
}