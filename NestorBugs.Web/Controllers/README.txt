If you add a new controller, make sure it inherits from the "ControllerBase" class, not from the "Controller" class.

Controller classes are instantiated via dependency injection (Unity), so you can set the required dependencies in the class constructor.

Also, take a look at the ControllerBase class since it has a couple of extra useful methods.