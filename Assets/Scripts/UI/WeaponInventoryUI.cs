using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Armas del jugador arriba a la izquierda, como en el juego original:
// la equipada va primero (a la izquierda) y las demás van a su derecha, una al lado de otra.
// Al cambiar de arma la fila se reordena a partir de la nueva equipada.
public class WeaponInventoryUI : MonoBehaviour
{
    public Transform container; // donde se crean los iconos
    public float iconSize = 48f; // en píxeles del lienzo 320x240; el sprite original mide 16 (48 = 3 veces)
    public float spacing = 2.25f; // espacio entre las armas no equipadas, en píxeles originales
    public float othersStartX = 53f; // dónde empieza la primera arma no equipada, en píxeles originales desde el borde del icono equipado

    [Header("Balas (TextBox.png)")]
    public Sprite slashSprite; // TextBox_10: "/"
    public Sprite dashSprite; // TextBox_11: "-" (munición ilimitada)
    public Sprite[] digitSprites; // TextBox_0 a TextBox_9, para cuando el arma tenga balas contadas
    public float ammoGap = 2f; // distancia entre el icono y la barra "/", en píxeles originales
    public Vector2 slashPos = new Vector2(0f, 9f); // posiciones en píxeles originales, relativas al bloque (y hacia abajo)
    public float ammoColumnX = 10f; // primera columna de guiones
    public float ammoColumnPitch = 8f; // distancia entre columnas
    public float ammoTopY = -1f; // fila de arriba (balas actuales)
    public float ammoBottomY = 8f; // fila de abajo (balas máximas)

    private GunController guns;
    private readonly List<Image> slots = new List<Image>(); // mismo orden que gunsOwned
    const int Columns = 3; // dígitos por fila (hasta 999)
    private readonly Image[] currentImages = new Image[Columns]; // fila de arriba: balas actuales (centenas, decenas, unidades)
    private readonly Image[] maxImages = new Image[Columns]; // fila de abajo: balas máximas
    private RectTransform ammoRoot; // bloque "-- / --" junto a la equipada

    void Start()
    {
        guns = FindFirstObjectByType<GunController>();
        if (guns == null)
            return;

        guns.OnGunChanged += Refresh;
        guns.OnAmmoChanged += Refresh;
        BuildSlots();
        BuildAmmo();
        Refresh();
    }

    void OnDestroy()
    {
        if (guns != null)
            guns.OnGunChanged -= Refresh;
            guns.OnAmmoChanged -= Refresh;
    }

    // un icono por cada arma que tiene el jugador
    void BuildSlots()
    {
        foreach (Gun gun in guns.gunsOwned)
        {
            GameObject slot = new GameObject(gun.name + " Icon", typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(container, false);

            Image image = slot.GetComponent<Image>();
            image.sprite = gun.inventoryIcon;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.enabled = gun.inventoryIcon != null; // sin icono, no se dibuja

            // anclado arriba a la izquierda del contenedor; la posición la pone Refresh
            RectTransform rect = (RectTransform)slot.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(iconSize, iconSize);
            slots.Add(image);
        }
    }

    // "/" y dos filas de dos guiones, como la munición ilimitada del original
    void BuildAmmo()
    {
        GameObject root = new GameObject("Ammo", typeof(RectTransform));
        root.transform.SetParent(container, false);
        ammoRoot = (RectTransform)root.transform;
        ammoRoot.anchorMin = ammoRoot.anchorMax = ammoRoot.pivot = new Vector2(0f, 1f);
        ammoRoot.sizeDelta = Vector2.zero;

        AddAmmoImage("Slash", slashSprite, slashPos);
        for (int column = 0; column < Columns; column++)
        {
            float x = ammoColumnX + column * ammoColumnPitch;
            currentImages[column] = AddAmmoImage("Current" + column, dashSprite, new Vector2(x, ammoTopY));
            maxImages[column] = AddAmmoImage("Max" + column, dashSprite, new Vector2(x, ammoBottomY));
        }
    }

    Image AddAmmoImage(string imageName, Sprite sprite, Vector2 pixelPos)
    {
        float scale = iconSize / 16f; // cada píxel del dibujo original
        GameObject go = new GameObject(imageName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(ammoRoot, false);

        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        image.enabled = sprite != null;

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(8f, 8f) * scale;
        rect.anchoredPosition = new Vector2(pixelPos.x, -pixelPos.y) * scale;
        return image;
    }

    // equipada a la izquierda; las otras a su derecha, siguiendo el orden de gunsOwned
    void Refresh()
    {
        if (guns == null || slots.Count == 0)
            return;

        int equipped = guns.EquippedGunID;
        Place(slots[equipped], 0f, 0f);

        // las balas van pegadas a la derecha de la equipada; solo se ven si el arma tiene munición ilimitada
        if (ammoRoot != null)
        {
            Gun gun = guns.gunsOwned[equipped];
            ammoRoot.gameObject.SetActive(gun.unlimitedAmmo || gun.usesAmmo);
            if (gun.usesAmmo)
            {
                ShowNumber(currentImages, guns.GetAmmo(equipped));
                ShowNumber(maxImages, guns.GetMaxAmmo(equipped));
            }
            else
            {
                ShowDashes(currentImages);
                ShowDashes(maxImages);
            }
            ammoRoot.anchoredPosition = new Vector2(iconSize + ammoGap * (iconSize / 16f), 0f);
        }

        // las demás van a la derecha de las balas, una al lado de otra (medidas en píxeles originales)
        float scale = iconSize / 16f;
        float x = othersStartX * scale;
        for (int k = 1; k < slots.Count; k++)
        {
            Place(slots[(equipped + k) % slots.Count], x, 0f);
            x += (16f + spacing) * scale;
        }
    }

    // "--" para las armas de munición ilimitada: ocupa las dos columnas de la derecha
    void ShowDashes(Image[] row)
    {
        for (int i = 0; i < row.Length; i++)
        {
            row[i].sprite = dashSprite;
            row[i].enabled = dashSprite != null && i >= row.Length - 2;
        }
    }

    // número alineado a la derecha; las columnas de la izquierda que sobran quedan vacías
    void ShowNumber(Image[] row, int value)
    {
        string digits = Mathf.Clamp(value, 0, (int)Mathf.Pow(10, row.Length) - 1).ToString();
        int offset = row.Length - digits.Length;
        for (int i = 0; i < row.Length; i++)
            SetDigit(row[i], i < offset ? -1 : digits[i - offset] - '0');
    }

    void SetDigit(Image image, int digit)
    {
        bool valid = digit >= 0 && digitSprites != null && digit < digitSprites.Length && digitSprites[digit] != null;
        image.enabled = valid;
        if (valid)
            image.sprite = digitSprites[digit];
    }

    void Place(Image slot, float x, float y)
    {
        ((RectTransform)slot.transform).anchoredPosition = new Vector2(x, y);
    }
}
