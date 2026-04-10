# 🎥 MÉTRŌMA  |  CAMERA SUITE

> [!NOTE]
> **Version 2.6 PRO**  
> *L'écosystème complet pour la chorégraphie cinématique et le feedback procédural.*

La **Metroma Camera Suite** est un outil hybride conçu pour offrir une liberté totale aux Game Designers (GD) via la Timeline, tout en fournissant une architecture extensible et robuste pour les Game Programmers (GP).

![Aperçu de la Camera Suite (Minimaliste)](/c:/Users/PIERRE/.gemini/antigravity/brain/86ab71b0-28e9-4ba9-ae2a-85c82773dcbf/camera_suite_overview_minimalist_1775740372325.png)

---

## 🎨 Guide de Mise en Route GD (Pas-à-Pas)

Ce guide vous aide à configurer votre première séquence cinématique en quelques minutes.

### 1. Créer le Rail (Spline)
Utilisez l'outil **Dreamteck Splines** pour dessiner le chemin de votre caméra dans la scène.
- Assurez-vous que le rail est fluide et évitez les angles trop brusques.
- Nommer vos rails (ex: **Rail.1-Chapter1**) pour mieux les organiser :
    - **Rail.1** correspond au numéro du rail dans le chapitre.
    - **Chapter1** correspond au nom du chapitre associé.

### 2. Configurer le Camera Tool
Ajoutez le composant **Camera Tool** à un objet vide dans votre scène (ou utilisez le prefab dédié).
- **Assignation** : Glissez votre Caméra principale dans le slot `Target Camera`.
- **Director** : Assignez le `Playable Director` qui pilotera la séquence.
- **Rails** : Ajoutez votre rail créé à l'étape 1 dans la liste `Spline Rails`.

### 3. Créer un Chapter
Dans l'inspecteur du Camera Tool, allez dans la section **Chapters**.
- Cliquez sur **+ Add Manual Chapter** ou utilisez **Scan Project** pour trouver vos Timelines.
- Donnez un nom clair à votre séquence (ex: "Boss_Intro").

### 4. Ajuster le Pacing & Segments
Votre rail est divisé en **Segments** (entre chaque nœud de la spline).
![Diagramme de Pacing (Minimaliste)](/c:/Users/PIERRE/.gemini/antigravity/brain/86ab71b0-28e9-4ba9-ae2a-85c82773dcbf/pacing_segments_minimalist_1775740390268.png)
- Dans l'onglet **Pacing & Segments**, réglez la **Duration** pour chaque segment.
- Choisissez une courbe d'**Easing** (ex: `Ease-In-Out` pour un mouvement naturel).
- Utilisez **Wait at End** si vous voulez que la caméra marque une pause sur un nœud précis avant de repartir.

### 5. Génération de la Timeline
Une fois votre pacing réglé, cliquez sur le bouton **🎬 GENERATE TIMELINE CLIPS**.
- Cela va créer automatiquement une **Camera Track** dans votre Timeline et y placer des clips synchronisés avec vos segments.
- **Nomenclature** : Le fichier généré suit le format **`TL-[NomDuChapitre].playable`** pour une organisation optimale.

### 6. Ajouter du "Juice" avec les Markers
Sur votre Timeline, faites un clic droit sur la piste de caméra pour ajouter des **Markers**.
![Aide Mémoire Markers (Minimaliste)](/c:/Users/PIERRE/.gemini/antigravity/brain/86ab71b0-28e9-4ba9-ae2a-85c82773dcbf/marker_cheat_sheet_minimalist_1775740406119.png)
- **💥 Shake** : Pour un impact ou une vibration.
- **🎥 Dolly Zoom** : Pour l'effet "Vertigo" sur un moment clé.
- **🔍 Rack Focus** : Pour changer la mise au point entre deux sujets.

### 7. Preview & Finalisation
Utilisez le bouton **▶ Preview Animation** dans l'inspecteur pour voir le résultat sans lancer le jeu.
- Activez la **Rule of Thirds** pour parfaire votre cadrage.
- Verrouillez la vue avec **📍 Lock Camera**.

---

## 🔔 La Bibliothèque de Markers

| Marker | Fonction | Propriétés Clés |
| :--- | :--- | :--- |
| **💥 Shake** | Déclenche une vibration procédurale (Perlin). | `Intensity`, `Duration`, `Roughness` |
| **🎥 Dolly Zoom** | Effectue un zoom compensé (Effet Vertigo). | `PushDistance`, `TargetFOV`, `Curve` |
| **🎯 LookAt Switch**| Change instantanément de cible de tracking. | `Index`, `TransitionDuration` |
| **🎞 Next Chapter** | Enchaîne vers une autre Timeline. | `ChapterName`, `BlendDuration` |
| **⏳ Slow Motion** | Modifie l'échelle de temps globale. | `TimeScale`, `Duration` |
| **✨ Flash** | Overlay de couleur (Flashbang, Dégâts). | `Color`, `Duration` |
| **🔍 Rack Focus** | Change la mise au point (Profondeur de champ). | `TargetDistance`, `Duration` |
| **🛠 Rail Switch** | Change de rail de spline en cours de route. | `RailIndex` |

---

## 💻 Guide de Référence Technique (GP)

### 1. Architecture Singleton & Accès Services
Le système utilise un **Singleton** global pour un accès simplifié depuis n'importe quel script de gameplay.
- **`CameraTool.Active`** : L'instance principale du gestionnaire.
- **`DefaultExecutionOrder(100)`** : Le gestionnaire applique les effets après l'update standard pour assurer la stabilité.

---

### 2. Référence API : Méthodes Publiques (`CameraTool`)
Voici l'intégralité des commandes de contrôle disponibles :

| Méthode | Paramètres | Description |
| :--- | :--- | :--- |
| **`PlayChapter`** | `(string name/int idx, float blendDuration)` | Change le chapitre actif, re-bind la Timeline et transitionne vers le rail. |
| **`TransitionToPose`** | `(CameraPose target, float duration, AnimationCurve curve, DirectorAction action)` | Détache la caméra du rail pour aller vers une vue fixe (`StaticPose`). |
| **`ReturnToRail`** | `(float duration, AnimationCurve curve)` | Ré-attache la caméra au rail de manière fluide. |
| **`SwitchToRail`** | `(int idx)` | Bascule le focus sur un rail de spline spécifique (mode `SingleRailMode`). |
| **`ResetToChainMode`** | - | Repasse en mode chaîne (évalue tous les rails bout à bout). |
| **`SetLookAt`** | `(Transform target, float duration)` | Change la cible de tracking du `LookAtWeight` avec interpolation. |
| **`TriggerTimelineEvent`**| `(string eventName)` | Déclencheur générique pour effets nommés (ex: "flash", "wobble"). |

---

### 3. Catalogue des Méthodes d'Extensions (`CameraModifiers`)
Ces méthodes sont accessibles sur n'importe quel objet `UnityEngine.Camera`.
![Pile de Feedback (Minimaliste)](/c:/Users/PIERRE/.gemini/antigravity/brain/86ab71b0-28e9-4ba9-ae2a-85c82773dcbf/fx_stack_minimalist_1775740436422.png)

#### Vibrations & Shakes
- **`DoShake(intensity, duration, roughness, fadeOut)`** : Shake Perlin classique.
- **`DoShakeAt(sourcePos, maxIntensity, duration, radius, falloff)`** : Shake atténué par la distance.
- **`DoImpact(direction, intensity, duration)`** : Impact directionnel unique (Recoil, Atterrissage).

#### Optiques & Lentilles
- **`DoFOVTransition(targetFOV, duration, curve)`** : Transition de champ de vision.
- **`DoFOVPulse(amplitude, frequency, duration)`** : Effet de respiration FOV.
- **`DoDollyZoom(pushDistance, targetFOV, duration, curve)`** : Effet Vertigo.
- **`DoDepthOfFieldFocus(targetDist, duration, curve)`** : Rack focus via Bloom/DoF (URP).
- **`SetAutoFocus(enabled, target)`** : Active le focus dynamique en temps réel.

#### Offsets Procéduraux
- **`AddPositionOffset(offset, duration, curve)`** : Ajoute un décalage XYZ temporaire.
- **`AddRotationOffset(euler, duration, curve)`** : Ajoute une rotation Euler temporaire.
- **`SetHandheld(active, amplitude, frequency)`** : Active l'effet organique de "caméra à l'épaule".
- **`DoWobble(amplitude, frequency, duration)`** : Oscillation cyclique (Drunk/Poison).
- **`SetDutchAngle(targetAngleZ, duration, curve)`** : Modifie le Tilt de la caméra.

#### Screen & Time Effects
- **`DoFlash(color, duration)`** : Flash plein écran via overlay UI.
- **`DoHitStop(duration, timeScale)`** : Gel du temps interactif (`TimeScale = 0.05f` par défaut).
- **`StopAllCameraModifiers()`** : Killswitch immédiat pour tous les effets.
- **`ClearOffsetsSmoothly(fadeDuration)`** : Retour à zéro fluide de tous les offsets.

---

### 4. Delegates & Système d'Événements
Abonnez-vous à ces événements pour synchroniser votre logique système (UI, sons, IA).

| Événement | Type d'Action | Timing de Déclenchement |
| :--- | :--- | :--- |
| **`OnChapterStarted`** | `CameraChapter` | Appelé à l'instant du changement de chapitre (avant transition). |
| **`OnChapterActive`** | `CameraChapter` | Appelé quand le chapitre est pleinement "on-rail" (après transition). |
| **`OnStateChanged`** | `CameraState` | Appelé à chaque changement d'état de la machine (ex: Transition -> FollowRail). |
| **`OnMarkerEventHit`** | `CameraMarkerBase` | Notifie quel Marker vient d'être atteint sur la Timeline. |

---

### 5. Guide d'Extension : Custom Markers
Utilisez le **Command Pattern** pour étendre le système sans toucher au code source.
![Architecture GP (Minimaliste)](/c:/Users/PIERRE/.gemini/antigravity/brain/86ab71b0-28e9-4ba9-ae2a-85c82773dcbf/gp_workflow_minimalist_1775740422699.png)

```csharp
[System.Serializable]
[DisplayName("Camera/My Custom FX")]
public class MyEffectMarker : CameraMarkerBase
{
    public float power = 2.0f;
    public override void Execute(CameraTool tool) {
        tool.TargetCamera.DoShake(power, 0.5f);
    }
}
```

---

> [!IMPORTANT]
> **Performance Architecture** : Le `CameraModifierHandler` utilise des pools internes de **4 slots par type d'effet** (Shakes, Impacts, Offsets). Cela garantit **zéro allocation de mémoire (GC)** pendant le runtime, crucial pour éviter les micro-saccades.
