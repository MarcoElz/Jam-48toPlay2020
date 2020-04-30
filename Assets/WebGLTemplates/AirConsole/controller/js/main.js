jQuery(document).ready(function() {
  var context = location.hash === "#editor" ? ControllerGenerator.Context.Editor : ControllerGenerator.Context.AirConsole;
  var is_airconsole_ctx = context === ControllerGenerator.Context.AirConsole;
  var generator = new ControllerGenerator(context);
  var parsed_data = {};
  var airconsole = null;




  


  // ==========================================
  // AIRCONSOLE
  // ==========================================
  if (is_airconsole_ctx) {

    // Construct2, send handshake message to the contruct2 game
    function sendHandshake() {
      airconsole.message(AirConsole.SCREEN, {
        handshake: true
      });
    };

    if (!ctrl_data) {
      throw "Controller data is missing. Did you copy all the export data?";
    }

    parsed_data = JSON.parse(ctrl_data);

    airconsole = new AirConsole({
      orientation: parsed_data.orientation || AirConsole.ORIENTATION_PORTRAIT
    });

	ViewManager = new AirConsoleViewManager(airconsole);
	

	  //airconsole.getPremium();

    airconsole.onReady = function() {
      generator.applyData(parsed_data);
      if (parsed_data.selected_view_id) {
        //generator.setCurrentView(parsed_data.selected_view_id);
      }
      // Construct2
      sendHandshake();
    };

    airconsole.onMessage = function(device_id, data) {
      generator.onAirConsoleMessage(device_id, data);
      // Construct2
      if (data.handshake) {
        sendHandshake();
      }
	  /* OLD
	  if(data == "none")
	  {
		$("#view-0").html('<div style="position: absolute;left: 50%;top: 50%;transform: translate(-50%, -50%);" > <font color="white">Max number of players in game. You can not play.</font> </div>');
	  }
	  else
	  {
		$("#view-0").css("background-color", data);
	  }
	  */
    };

	airconsole.onCustomDeviceStateChange = function(device_id, device_data){
	//to see logs from the controller, start your game in the "virtual controllers" browser start mode from Unity and open your browser's developer console. 
        console.log("onCustomDeviceStateChange", device_id, device_data);

        //check if the device that sent the custom device state change is the Screen (i.e. the Unity Game, in this case), and not one of the other controllers
        if (device_id == AirConsole.SCREEN){
          //check if the CustomDeviceStateChange data contains any view data
          if (device_data["view"] != null && device_data["view"] != ""){
            //set a new view accordingly
            ViewManager.show(device_data["view"]);
          }
          
          //check if there's any player color data
          if (device_data["playerColors"] != null){
            //check the CustomDeviceStateChange data contains any playerColorData for this particular device
            if (device_data["playerColors"][airconsole.getDeviceId()]){
              //this works for named colors. If you want to use colors that don't have a name, you could pass a hex code instead of a string/name
              //document.getElementById("background").style.backgroundColor = device_data["playerColors"][airconsole.getDeviceId()];
			  //$("#background").css("background-color", device_data["playerColors"][airconsole.getDeviceId()]);

			  if(device_data["playerColors"][airconsole.getDeviceId()] == "none")
			  {
				$("#control").html('<div style="position: absolute;left: 50%;top: 50%;transform: translate(-50%, -50%);" > <font color="white">Max number of players in game. You can not play.</font> </div>');
			  }
			  else
			  {
				$("#control").css("background-color", device_data["playerColors"][airconsole.getDeviceId()]);
				//$("#control-2").css("background-color", device_data["playerColors"][airconsole.getDeviceId()]);
			   }

			  
            }
          }
        }
      };

  // ==========================================
  // EDITOR
  // ==========================================
  } else {
    window.addEventListener('message', generator.onMessage.bind(generator));
    generator.preloadTemplates(function() {
      if (generator.last_build_data) {
        generator.onUpdate(generator.last_build_data);
      }
    });
  }

  /**
   * Gets called whenever an input element was pressed
   * @param {String} id
   * @param {Object} data
   */
  generator.onInputEvent = function(id, data) {
    var msg = this.formatMessage(id, data);
    if (is_airconsole_ctx) {
      airconsole.message(AirConsole.SCREEN, msg);
    } else {
      window.parent.postMessage({
        action: 'log',
        element_id: id,
        msg: msg
      }, "*");
    }
  }
});
