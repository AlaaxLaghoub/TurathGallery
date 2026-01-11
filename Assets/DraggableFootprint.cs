using UnityEngine;

public class DraggableFootprint : MonoBehaviour
{
    public AnimalType footprintType;

  //  public GameObject footprints;


    private Vector3 startPosition;
    private bool isDragging;
    private bool goingBack;

//    public Sprite normalSprite;
    public SpriteRenderer correctAnimal;
    public Sprite correctSprite;


    AnimalSlot slot;
    void Start()
    {
        startPosition = transform.position;
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;
    }

    void OnMouseUp()
    {
        isDragging = false;

        CheckSlot();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<AnimalSlot>()  == null)
        {
            return;
        }

     
        slot = collision.gameObject.GetComponent<AnimalSlot>();

       
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<AnimalSlot>() == null)
        {
            return;
        }

        slot = null;
    }

    private void CheckSlot()
    {
        if (slot != null && !slot.isOccupied)
        {
            if (slot.animalType == footprintType)
            {
                // Correct match
                transform.position = slot.snapPoint.position;
                slot.isOccupied = true;
                GetComponent<Collider2D>().enabled = false;
                enabled = false;

                correctAnimal.sprite = correctSprite;
                gameObject.SetActive(false);


                GameManager.Instance.CheckWin();
            }
            else
            {
                // Wrong match
                ResetPosition();
            }
        }
    }

    void ResetPosition()
    {
        goingBack = true;
        Debug.Log("Go Back to start");
        transform.position = startPosition;
        goingBack = false;
    }
}
