MyAlbum is an UWP MVVM app for viewing and organizing images. It was built with VS 2015 update 3.

![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/StartUp.gif "StartUp")

### Featuers:
+ JSON Serialization - Auto save changes in Windows isolated storage.
+ MVVM - Implemented independently, without using any 3rd party library.
+ Binding - using the newer "x:bind".
+ Variable Sized Wrap Grid View* (for displaing names of characters in photo details side panel)
* Based on [Jerry Nixon's blog post](http://blog.jerrynixon.com/2012/08/windows-8-beauty-tip-using.html)
+ Back/Forward navigation
+ Windows Location Service
+ Adaptive Display
+ Data Validations using Data Annotations
+ Menu Flyouts
+ Content dialogs
+ Custom message dialog
+ Live Tiles
+ Dark/Light Themes
		
...and many other tricks & goodies :)
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/AdaptiveDisplay.gif "Adaptive Display")
		
The application consists of 3 projects:
+ **MyAlbum** - GUI Client (UWP application)
+ **MyAlbum.BL** - Business logic layer (UWP class library)
+ **MyAlbum.DAL** - Data access layer (UWP class library)

### Operations:
#### 1. Add/Delete album
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/NewAlbum.gif "New Album")
		
	![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/DeleteAlbum.gif "Delete Album")
#### 2. Edit album properties (name & "is main?")
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/ChangeMainAlbum.gif "Edit Album")
#### 3. Add/Delete photos (file picker, drag & drop, camera)
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/DeletePhoto.gif "Delete Photo")
#### 4. Edit photo details in 2 places - directly in photo card or in the side panel
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/EditPhotoDetails.gif "Edit Photo Details")
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/Validation.gif "Validation")
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/Location.gif "Set Location")
#### 5. Move photos between albums
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/MovePhoto.gif "Move Photo")
#### 6. Add/Delete characters in photo
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/AddCharacter.gif "Add Character")
#### 7. Toggle between Grid view and Flip view
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/FlipView.gif "Toggle View")
#### 8. Toggle between Dark theme and Light theme
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/Theme.gif "Toggle Theme")
#### 9. Move Back/Forward
		
![alt text](https://github.com/PrisonerM13/MyAlbum/blob/master/MoveBack.gif "Move Back")

> Main album is displayed when the app starts and its name is displayed in bold in "Albums" menu.
		
> Current album is identified by a check mark in "Albums" menu.

### Menu
**Albums** - Navigation between albums or creation of a new one
- Album 1
- Album 2
- Album 3
- (...)
- **New Album...**
		
**Add Photos...** - Open file picker to add photos to current album
		
**Capture** - Open camera app to take a picture and add it to current album
		
**Photo**
- **Details** - Open photo details side panel (if not open already)
- **Move To...** - Move selected photos to another album
- **Delete** - Delete selected photos
		
**Exit**
			
### Models:
+ Model<T> - An abstract base class for Album & Photo models.
+ Album - Reflects an Album as it exists in storage. Contains a collection of photos.
+ Photo - Reflects a Photo as it exists in storage. Contains a collection of characters
+ Character - Reflects a character in photo (resembles a facebook tag, but simpler). Saved as path of its Photo container.

### View Models
+ ViewModelBase<T> - An abstract base class for Album & Photo ViewModels, where T is the model.
+ MainViewModel - Controlls Main view
+ AlbumViewModel - Represents and controlls an Album model and contains a collection of PhotoViewModels
+ PhotoViewModel - Represents and controlls a Photo model and contains a collection of CharacterViewModels
+ CharacterViewModel - Represents a Character model

### Views
+ Main: Container for all other views. Hosts app title, menu, navigation buttons and theme button. The rest is a frame used as a placeholder for other views (currently only album).
+ Album: Displays photos in a grid or flip view. Contains the photos edit side panel, which can be pinned.
