using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class WordSlot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image background;       // slot background Image (square)
    public TMP_Text letterText;    // child TMP text showing the letter
    public Outline outline;        // optional outline to highlight slot

    bool draggable = true;
    CanvasGroup canvasGroup;

    // drag-moving tile
    GameObject dragTile;          // the visual tile that follows the pointer
    RectTransform dragTileRect;
    TMP_Text dragTileText;
    Canvas rootCanvas;

    // store the letter we picked up (so we can restore if drop fails)
    string pickedLetter = "";

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (outline != null) outline.enabled = false;
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Setup(char letter)
    {
        if (letterText != null) letterText.text = letter.ToString();
    }

    public char GetLetter()
    {
        return string.IsNullOrEmpty(letterText.text) ? ' ' : letterText.text[0];
    }

    public void SetGreen()
    {
        if (background != null) background.color = Color.green;
        draggable = false;
    }

    public void SetYellow()
    {
        if (background != null) background.color = Color.yellow;
        draggable = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!draggable) return;
        if (letterText == null) return;

        pickedLetter = letterText.text;
        letterText.text = "";

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;

        if (rootCanvas != null)
        {
            dragTile = new GameObject("DragTile", typeof(RectTransform));
            dragTile.transform.SetParent(rootCanvas.transform, false);
            dragTileRect = dragTile.GetComponent<RectTransform>();
            dragTileRect.pivot = new Vector2(0.5f, 0.5f);


            Image img = dragTile.AddComponent<Image>();
            if (background != null && background.sprite != null)
            {
                img.sprite = background.sprite;
                img.type = background.type;
                img.preserveAspect = background.preserveAspect;
            }
            else if (background != null)
            {
                img.color = background.color;
            }
            img.raycastTarget = false; 


            GameObject tgo = new GameObject("TileText", typeof(RectTransform));
            tgo.transform.SetParent(dragTile.transform, false);
            dragTileText = tgo.AddComponent<TMP_Text>();

            if (letterText != null)
            {
                dragTileText.font = letterText.font;
                dragTileText.fontSize = letterText.fontSize;
                dragTileText.fontStyle = letterText.fontStyle;
                dragTileText.color = letterText.color;
                dragTileText.alignment = letterText.alignment;
                dragTileText.enableAutoSizing = letterText.enableAutoSizing;
                dragTileText.enableWordWrapping = false;
                dragTileText.text = pickedLetter;
            }


            if (background != null && background.rectTransform != null)
                dragTileRect.sizeDelta = background.rectTransform.sizeDelta;
            else
                dragTileRect.sizeDelta = new Vector2(100, 100);


            dragTile.transform.SetAsLastSibling();

            UpdateDragTilePosition(eventData);
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (dragTile == null) return;
        UpdateDragTilePosition(eventData);
    }


    public void OnEndDrag(PointerEventData eventData)
    {

        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;


        if (dragTile != null)
        {
            Destroy(dragTile);
            dragTile = null;
            dragTileRect = null;
            dragTileText = null;
        }


        if (string.IsNullOrEmpty(letterText.text) && !string.IsNullOrEmpty(pickedLetter))
        {
            letterText.text = pickedLetter;
        }


        pickedLetter = "";
    }


    public void OnDrop(PointerEventData eventData)
    {

        var sourceGO = eventData.pointerDrag;
        if (sourceGO == null) return;

        WordSlot sourceSlot = sourceGO.GetComponent<WordSlot>();
        if (sourceSlot == null) return;


        if (sourceSlot == this) return;
        if (!this.draggable || !sourceSlot.draggable) return;


        string sourceLetter = sourceSlot.pickedLetter;
        string targetLetter = this.letterText.text;

        this.letterText.text = sourceLetter;
        sourceSlot.letterText.text = targetLetter;

        sourceSlot.pickedLetter = "";

        if (sourceSlot.dragTile != null)
        {
            Destroy(sourceSlot.dragTile);
            sourceSlot.dragTile = null;
            sourceSlot.dragTileRect = null;
            sourceSlot.dragTileText = null;
        }

        if (WordGameManager.Instance != null)
            WordGameManager.Instance.CheckWord();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (outline != null) outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outline != null) outline.enabled = false;
    }

    void UpdateDragTilePosition(PointerEventData eventData)
    {
        if (dragTileRect == null || rootCanvas == null) return;

        RectTransform canvasRect = rootCanvas.transform as RectTransform;
        Vector2 localPoint;
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, null, out localPoint);
            dragTileRect.anchoredPosition = localPoint;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, rootCanvas.worldCamera, out localPoint);
            dragTileRect.anchoredPosition = localPoint;
        }
    }
}