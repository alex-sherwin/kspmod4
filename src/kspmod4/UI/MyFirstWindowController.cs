using KSP.UI.Binding;
using kspmod4.Unity.Runtime;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace kspmod4.UI;

/// <summary>
/// Controller for the MyFirstWindow UI.
/// </summary>
public class MyFirstWindowController : MonoBehaviour
{
    // The UIDocument component of the window game object
    private UIDocument _window;

    // The elements of the window that we need to access
    private VisualElement _rootElement;
    private TextField _nameTextfield;
    private Toggle _noonToggle;
    private Label _greetingLabel;
    private TreeView _myTree;

    // The backing field for the IsWindowOpen property
    private bool _isWindowOpen;

    /// <summary>
    /// The state of the window. Setting this value will open or close the window.
    /// </summary>
    public bool IsWindowOpen
    {
        get => _isWindowOpen;
        set
        {
            _isWindowOpen = value;

            // Set the display style of the root element to show or hide the window
            _rootElement.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            // Alternatively, you can deactivate the window game object to close the window and stop it from updating,
            // which is useful if you perform expensive operations in the window update loop. However, this will also
            // mean you will have to re-register any event handlers on the window elements when re-enabled in OnEnable.
            // gameObject.SetActive(value);

            // Update the Flight AppBar button state
            GameObject.Find(kspmod4Plugin.ToolbarFlightButtonID)
                ?.GetComponent<UIValue_WriteBool_Toggle>()
                ?.SetValue(value);

            // Update the OAB AppBar button state
            GameObject.Find(kspmod4Plugin.ToolbarOabButtonID)
                ?.GetComponent<UIValue_WriteBool_Toggle>()
                ?.SetValue(value);
        }
    }

    /// <summary>
    /// Runs when the window is first created, and every time the window is re-enabled.
    /// </summary>
    private void OnEnable()
    {
        // Get the UIDocument component from the game object
        _window = GetComponent<UIDocument>();

        // Get the root element of the window.
        // Since we're cloning the UXML tree from a VisualTreeAsset, the actual root element is a TemplateContainer,
        // so we need to get the first child of the TemplateContainer to get our actual root VisualElement.
        _rootElement = _window.rootVisualElement[0];

        // Get the text field from the window
        _nameTextfield = _rootElement.Q<TextField>("name-textfield");
        // Get the toggle from the window
        _noonToggle = _rootElement.Q<Toggle>("noon-toggle");
        // Get the greeting label from the window
        _greetingLabel = _rootElement.Q<Label>("greeting-label");

        _myTree = _rootElement.Q<TreeView>("mytree");

        setupTree();

        // Center the window by default
        _rootElement.CenterByDefault();

        // Get the close button from the window
        var closeButton = _rootElement.Q<Button>("close-button");
        // Add a click event handler to the close button
        closeButton.clicked += () => IsWindowOpen = false;

        // Get the "Say hello!" button from the window
        var sayHelloButton = _rootElement.Q<Button>("say-hello-button");
        // Add a click event handler to the button
        sayHelloButton.clicked += SayHelloButtonClicked;
    }

    private void setupTree()
    {
        var items = new List<TreeViewItemData<string>>(110);
        for (var i = 0; i < 10; i++)
        {
            var itemIndex = i * 10 + i;

            var treeViewSubItemsData = new List<TreeViewItemData<string>>(10);
            for (var j = 0; j < 10; j++)
                treeViewSubItemsData.Add(new TreeViewItemData<string>(itemIndex + j + 1, (j + 1).ToString()));

            var treeViewItemData = new TreeViewItemData<string>(itemIndex, (i + 1).ToString(), treeViewSubItemsData);
            items.Add(treeViewItemData);
        };

        // The "makeItem" function will be called as needed
        // when the TreeView needs more items to render
        Func<VisualElement> makeItem = () => new Label();

        // As the user scrolls through the list, the TreeView object
        // will recycle elements created by the "makeItem"
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list)
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            var item = _myTree.GetItemDataForIndex<string>(i);
            (e as Label).text = item;
        };

        _myTree.SetRootItems(items);
        _myTree.makeItem = makeItem;
        _myTree.bindItem = bindItem;
        _myTree.selectionType = SelectionType.Multiple;
        _myTree.Rebuild();

    }

    private void SayHelloButtonClicked()
    {
        // Get the value of the text field
        var playerName = _nameTextfield.value;
        // Get the value of the toggle
        var isAfternoon = _noonToggle.value;

        // Get the greeting for the player from the example script in our Unity project assembly we loaded earlier
        var greeting = ExampleScript.GetGreeting(playerName, isAfternoon);

        // Set the text of the greeting label
        _greetingLabel.text = greeting;
        // Make the greeting label visible
        _greetingLabel.style.display = DisplayStyle.Flex;
    }
}
