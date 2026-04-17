# 🎥 MÉTRŌMA  |  CAMERA SUITE
> **MANUEL D'UTILISATION (GD & DESIGN)**  
> *Guide Complet d'Autonomie.*

---

## 🚀 1. INSTALLATION & SETUP

Il existe deux façons de configurer le tool : la méthode **Express** (recommandée) et la méthode **Manuelle**.

### A. L'Approche Express (Automatisation ⚡)
Le tool est conçu pour scanner votre scène et tout configurer tout seul si vous respectez la nomenclature.
1. **Préparez vos Rails** : Nommez vos splines `Rail.0-MonNom`, `Rail.1-MonNom`, etc.
2. **Posez le Prefab** : Glissez le prefab **Camera Tool** dans la scène.
3. **Cliquez sur ⟳ SCAN** (Onglet Chapters) :
   - Le tool va chercher tous les rails dans la scène.
   - Il va créer les **Chapters** correspondants.
   - Il va tenter de trouver la **Timeline** associée si elle porte le même nom.
4. **Utilisez 🎬 FOCUS** : Cliquez sur ce bouton à côté d'un chapitre pour que le tool assigne automatiquement la bonne Timeline au `Playable Director` et ouvre la fenêtre Timeline instantanément.

### B. L'Approche Manuelle (Pas-à-pas 🛠️)
Si vous avez une configuration spécifique :
1. **Target Camera** : Glissez votre caméra (MainCamera) dans ce slot.
2. **Director** : Glissez l'objet qui contient votre composant `Playable Director`.
3. **Spline Rails** : Ajoutez manuellement vos rails dans la liste. L'ordre dans la liste est l'ordre de la séquence.
4. **Chapters** : Cliquez sur `+ Add Manual Chapter` et configurez :
   - **Start Rail Index** : Le numéro du premier rail de cette séquence.
   - **Rail Count** : Combien de rails cette séquence utilise à la suite.

---

## ⏱️ 2. CONFIGURATION DU PACING (LE CŒUR DU TOOL)

### A. Régler les Segments
Dans l'onglet **Pacing & Segments**, chaque rail est découpé en segments (l'espace entre deux points de votre spline).
- **Duration** : Le temps (en secondes) pour parcourir ce segment précis.
- **Easing** : La courbe de vitesse (ex: `Linear` pour une vitesse constante, `EaseInOut` pour un départ/arrêt doux).
- **Wait At End** : Temps de pause à la fin du segment avant de passer au suivant.

### B. Gérer les Transitions (Jonctions Orange)
Le système utilise des **Overlaps Interstitiels**. Cela signifie que le temps de transition est **ajouté** entre les rails pour ne pas accélérer ou ralentir votre animation de base.
1. Allez dans les réglages de **Junction Override** d'un segment.
2. Réglez la **Duration** de la jonction.
3. Une zone **Orange** apparaîtra sur la Timeline lors de la génération. C'est la zone où la caméra passe d'un rail à l'autre en douceur.

---

## 🎬 3. GÉNÉRATION & TIMELINE

### A. Le bouton Magique
Une fois votre pacing réglé, cliquez sur **🎬 GENERATE TIMELINE CLIPS**.
- Le tool va créer/nettoyer automatiquement une piste **Camera Sequence Track**.
- Il va placer tous les clips avec la durée exacte calculée (Temps des segments + Temps des jonctions).

### B. Légende Visuelle sur la Timeline
- **Clip Bleu Foncé** : Rail Pair (0, 2, 4...).
- **Clip Turquoise (Teal)** : Rail Impair (1, 3, 5...).
- **Overlay Orange (J)** : Zone de jonction/transition. **Ne posez pas de Markers ici**, c'est une zone de mélange technique.

---

## 💥 4. EFFETS & MARKERS (JUICE)

Sur votre Timeline, faites un **Clic Droit > Add Marker** sur la piste caméra.
- **Shake** : Intensité et durée d'une vibration.
- **Flash** : Un éclat de couleur (parfait pour les impacts).
- **Dolly Zoom** : Effet de focale (Vertigo).
- **Rack Focus** : Change la mise au point entre deux distances.

---

## 🛠️ 5. DÉPANNAGE (FAQ GD)

| Problème | Solution |
| :--- | :--- |
| **La caméra ne bouge pas** | Vérifiez que le `Playable Director` est bien sur **Play On Awake** ou lancé par script. |
| **La transition est trop brusque** | Augmentez la durée de la **Junction** dans le dernier segment du rail précédent. |
| **Erreur "Invalid Playable Asset"** | Cliquez à nouveau sur **GENERATE TIMELINE CLIPS**. Le tool nettoiera et resynchronisera tout. |
| **La caméra fait n'importe quoi** | Vérifiez que vos rails sont bien dans le bon ordre dans la liste `Spline Rails`. |
| **Le bouton Generate est grisé** | Assurez-vous d'avoir sélectionné un Chapitre valide et d'avoir une Timeline assignée. |

---

> [!CAUTION]
> **Ne modifiez jamais la durée des clips manuellement sur la Timeline.**  
> Si vous voulez changer la vitesse, faites-le dans l'inspecteur du **Camera Tool** (Pacing) et re-cliquez sur **GENERATE**. Toute modif manuelle sur la Timeline sera écrasée au prochain rafraîchissement.
