# MyAlbum
### A UWP MVVM App for viewing and managing albums and images, built with VS 2015 update 3.
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/FilePicker.gif "Pick Files")

## Features/Technologies/Tools
+ JSON Serialization - Auto save changes in Windows isolated storage.
+ MVVM - Implemented independently, without using any 3rd party library.
+ Binding - using the newer "x:bind".
+ Variable Sized Wrap Grid View (for display of characters in side panel).
	- Inspired by [Jerry Nixon's blog post](http://blog.jerrynixon.com/2012/08/windows-8-beauty-tip-using.html)
+ Back/Forward navigation
+ Windows Location Service
+ Adaptive Display
+ **Data Validations using Data Annotations**
+ Content dialogs
+ Menu Flyouts
+ Custom message dialog
+ Live Tiles
+ Dark/Light Themes
		
...and many other tricks & goodies :)
		
The application consists of 3 projects:
+ **MyAlbum** - GUI Client (UWP application)
+ **MyAlbum.BL** - Business logic layer (UWP class library)
+ **MyAlbum.DAL** - Data access layer (UWP class library)

#### First Run
![Live Tiles](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/StartUp.gif "Live Tiles")

#### Adaptive Display
![Adaptive Display](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/AdaptiveDisplay.gif "Adaptive Display")

## Operations
### 1. Add/Delete album
		
#### Add Album
![New Album](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/NewAlbum.gif "New Album")
		
#### Delete Album
![Delete Album](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/DeleteAlbum.gif "Delete Album")

### 2. Edit album properties (name & "is main?")
![Edit Album](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/ChangeMainAlbum.gif "Edit Album")
- Main album is displayed when the app starts and its name is displayed in bold in "Albums" menu.
- Current album is identified by a check mark in "Albums" menu.

### 3. Add/Delete photos (file picker, drag & drop, camera)
#### Drag & Drop
![Drag & Drop](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/Drag&Drop.gif "Drag & Drop")
		
#### Delete Photo
![Delete Photo](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/DeletePhoto.gif "Delete Photo")

### 4. Edit photo details in 2 places - directly in photo card or in side panel
#### Edit Photo Details
![Edit Photo Details](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/EditPhotoDetails.gif "Edit Photo Details")
		
#### Data Validation
![Validation](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/Validation.gif "Validation")
		
#### Set Location
![Set Location](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/Location.gif "Set Location")

### 5. Move photos between albums
![Move Photo](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/MovePhoto.gif "Move Photo")

### 6. Add/Delete characters in photo
![Add Character](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/AddCharacter.gif "Add Character")

### 7. Toggle between Grid view and Flip view
![Toggle View](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/FlipView.gif "Toggle View")

### 8. Toggle between Dark theme and Light theme
![Toggle Theme](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/Theme.gif "Toggle Theme")

### 9. Navigate using menu and Back/Forward buttons
![Navigation](https://github.com/PrisonerM13/MyAlbum/blob/master/gif/Navigation.gif "Navigation")

## Menu
**Albums** - Navigation between albums or creation of a new one
- **Main Album**
- Album X
- Album Y
		
	(...)
		
- New Album...
		
**Add Photos...** - Open file picker to add photos to current album
		
**Capture** - Open camera app to take a picture and add it to current album
		
**Photo**
- **Details** - Open photo details side panel (if not already open)
- **Move To...** - Move selected photos to another album
- **Delete** - Delete selected photos
		
**Exit**
			
## Models
+ Model&lt;T&gt; - An abstract base class for Album & Photo models.
+ Album - Reflects an Album as it exists in storage. Contains a collection of photos.
+ Photo - Reflects a Photo as it exists in storage. Contains a collection of characters
+ Character - Reflects a character in photo (resembles a facebook tag, but simpler). Saved as part of its Photo container.

![BL Diagram](https://github.com/PrisonerM13/MyAlbum/blob/master/images/BL-Diagram.png "BL Diagram")

## View Models
+ ViewModelBase&lt;T&gt; - An abstract base class for Album & Photo ViewModels, where T is the model.
+ MainViewModel - Controlls Main view
+ AlbumViewModel - Represents and controlls an Album model and contains a collection of PhotoViewModels
+ PhotoViewModel - Represents and controlls a Photo model and contains a collection of CharacterViewModels
+ CharacterViewModel - Represents a Character model

![UI Diagram](https://github.com/PrisonerM13/MyAlbum/blob/master/images/UI-Diagram.png "UI Diagram")

## Views
+ Main: Container for all other views. Hosts app title, menu, navigation buttons and theme button. The rest is a frame used as a placeholder for other views (currently only album).
+ Album: Displays photos in a grid or flip view. Contains the photos edit side panel, which can be pinned.

## DAL
![DAL Diagram](https://github.com/PrisonerM13/MyAlbum/blob/master/images/DAL-Diagram.png "DAL Diagram")
		
#### Storage Location: 
	%LOCALAPPDATA%\Packages\{GUID}\LocalState
#### Folders (~tables):
+ Albums
+ Photos
