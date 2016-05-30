Getting Started
If you are new to Dynamic MVC, follow the instructions in the DynamicMVC_ReadMe file.

Release Notes 2.0
-Dynamic MVC 2.0 has a new mobile responsive index view.
-DynamicEntity attribute has a new EntityType property that allows you to seperate your metadata from your entities.  This is useful if your model objects are in a seperate project or dll.
-Inheriting from DynamicController is no longer supported.  Attributes aiding in inheritence have been removed.  The best way to customize the controller for a specific entity is to simply copy it and rename the copy using mvc naming conventions.