using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MAZDA_MCTool.Messages;

public class GamePathChangedMessage(string value) : ValueChangedMessage<string>(value);