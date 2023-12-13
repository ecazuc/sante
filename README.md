Module Santé 2023

Groupe n°1, étudiants : 
Cazuc Ewen
Le Boulaire Erwan

Version de Unity : 2023.1.16f1

2 scènes : 
- test_vibration → prise en main de Unity et tests des schémas haptiques
- scenario_evaluation → scénario d’évaluation de l’interface

Scripts : 
- CollisionCounter → compte le nombre de collisions durant le jeu (à l’aide de l’objet utilisateur/CollisionCollider)
- CollisionDetecter → détecte des obstacles proches de l’utilisateur (à l’aide de 2 capsules collider en mode trigger) et lui indique son emplacement
- CsvWriter → produit les données de l’évaluation dans un fichier csv (cf partie Récupération des données du jeu)
- Driver → driver pour utiliser les moteurs
- Trajectory → décrit l’action (vibrations) des checkpoints de la partie guidée
- UserControl → définit les contrôles de l’utilisateur en jeu
- VibrationManager → définit les vibrations possibles des moteurs (schémas haptiques)

Simuler une déficience visuelle : 
Donner un des 3 shaders suivants à l’objet utilisateur/MainCamera/deficience_visuelle (dans le champ Materials du composant Mesh Renderer) : 
- Mat_atteinte_vision_centrale → atteinte de la vision centrale
- Mat_atteinte_voies_optiques → atteinte des voies optiques
- Mat_bruit → atteinte parcellaire

S’équiper du dispositif : 
- 1 bague par moteur
- moteur vert → auriculaire gauche (1)
- moteur orange → index gauche (2)
- moteur bleu → index droit (3)
- moteur marron → auriculaire droit (4)

Schémas haptiques en jeu (cf diapo pour des illustrations) : 
(&& = en même temps, / = puis)
- Avance : (2)&&(3)
- Fait un pas sur la droite : (3)&&(4)
- Fait un pas sur la gauche : (1)&&(2)
- Recule : (1)&&(4) / (2)&&(3)
- Oriente toi vers la droite : (3) / (4)
- Oriente toi vers la gauche : (2) / (1)
- Succès : (1) / (2) / (3) / (4) / (4) / (3) / (2) / (1)
- Echec : (1)&&(2)&&(3)&&(4)
- Obstacle à droite : (4)
- Obstacle à gauche : (1)

Contrôle de l’utilisateur en jeu (souris uniquement) : 
- Avancer : clic molette ou scroll vers le haut
- Reculer : scroll vers le bas
- Faire un pas à droite : clic droit
- Faire un pas à gauche : clic gauche
- Tourner la caméra : tourner la souris

Scénario d’évaluation : 
- Partie guidée : suivre les indications (dernière indication jusqu’à en avoir une nouvelle) de déplacement tout en restant sur la route
	- Départ : commencer par avancer sur qlq mètres jusqu’à ressentir la 1ère indication
	- cette partie utilise des sphères (checkpoint du dossier checkpoints) utilisant le script Trajectoire.
	- Arrivée : dans le hall, devant la porte principale du bâtiment informatique (pour passer la marche, faire un détour par l’herbe sur la gauche du hall, puis rejoindre le hall en longeant le mur, le succès vous y attend)
- Partie libre : se déplacer librement dans les couloirs du bâtiment informatique avec détection des obstacles
	- Départ : avant le lancement du test, déplacer l’utilisateur dans le hall
	- Limite de temps : 1 minute max
[ Une partie du scénario d’évaluation doit correspondre à un lancement du jeu (exemple : lancement du jeu > partie guidée > arrêt du jeu, puis, lancement du jeu > partie libre > arrêt du jeu) ]

Récupération des données du jeu dans le fichier Assets/Resources/expe.csv .
On y trouve : 
- l’identifiant du test
- ‘Succes’ ou ‘Echec’ si l’utilisateur est parvenu au bout de la partie guidée
- ‘-1’ si ‘Echec’, le temps mis (en secondes) pour la partie guidée si ‘Succes’
- le nombre de collisions

Avant de lancer le jeu, régler l’identifiant du test avec le champ ‘Id Usager’ du script Csv Writer de l’objet utilisateur.
