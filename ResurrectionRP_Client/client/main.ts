import * as alt from 'alt';
import * as native from 'natives';
import * as chat from 'client/chat/chat';
import * as speedometer from 'client/speedometer/speedometer';
import * as utils from 'client/utils';
import * as login from 'client/login/Login';

chat.initialize()
speedometer.initialize();
utils.initialize();
login.init();