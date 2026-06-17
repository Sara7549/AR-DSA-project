// QuizQuestionData.cs
// This MonoBehaviour auto-populates the three QuizQuestionBank assets
// with game-relevant questions at runtime (Editor or Play mode).
//
// Attach to any persistent GameObject (e.g. QuizManager's GameObject).
// In the Inspector assign stackBank, queueBank, linkedListBank.
//
// Questions deliberately reference what the player just experienced:
//   Stack   → bowls / stacking / LIFO
//   Queue   → parking lot / exit / FIFO
//   LinkedList → train cars / connections / traversal

using UnityEngine;
using System.Collections.Generic;

public class QuizQuestionData : MonoBehaviour
{
    [Header("Assign your three QuizQuestionBank assets here")]
    public QuizQuestionBank stackBank;
    public QuizQuestionBank queueBank;
    public QuizQuestionBank linkedListBank;

    private void Awake()
    {
        PopulateStackQuestions();
        PopulateQueueQuestions();
        PopulateLinkedListQuestions();
    }

    // -------------------------------------------------------------------------
    // STACK QUESTIONS  (bowl-stacking game)
    // -------------------------------------------------------------------------
    // -------------------------------------------------------------------------
    // STACK QUESTIONS  (bowl-stacking game)
    // Gameplay focus:
    // - Only top access
    // - LIFO behaviour
    // - Temporary storage
    // - Overflow
    // - Planning moves
    // - Consequences of stack restrictions
    // -------------------------------------------------------------------------
    private void PopulateStackQuestions()
    {
        if (stackBank == null) return;

        // -----------------------------------------------------------------
        // BASIC
        // Recognition + direct gameplay understanding
        // -----------------------------------------------------------------
        stackBank.basicQuestions = new List<QuizQuestion>
    {
        Q(
        "In the game, why could you only move the TOP bowl?",
        "The game permanently locked bottom bowls",
        "Because only the bowl on top could be accessed directly",
        "Because the game randomly selected which bowl to unlock",
        "Because lower bowls disappear temporarily",
        1,
        "Stacks follow the LIFO rule (Last In, First Out). "
        + "Only the top element can be directly accessed."
        ),

        Q(
        "You placed a red bowl, then a blue bowl onto the same stack. "
        + "Which bowl could you remove first?",
        "The red bowl",
        "Either bowl",
        "The blue bowl",
        "Neither bowl",
        2,
        "The blue bowl was added last, so it is on top. "
        + "Stacks remove the most recently added item first."
        ),

        Q(
        "In the bowl game, what does LIFO mean?",
        "The first bowl added leaves first",
        "Bowls can leave in any order",
        "The last bowl added is the first removed",
        "Only large bowls can be moved",
        2,
        "LIFO stands for Last In, First Out. "
        + "The most recently added bowl is removed first."
        ),

        Q(
        "A stack already had 4 bowls. What happened when you tried "
        + "to add another bowl?",
        "The bottom bowl was removed automatically",
        "The stack became larger automatically",
        "The game would not let you place another bowl because the stack was full",
        "The bowls were rearranged",
        2,
        "Trying to add to a full stack causes stack overflow. "
        + "The game prevented the move."
        ),

        Q(
        "What made reaching a bowl near the bottom of the stack time-consuming?",
        "Bottom bowls were physically locked",
        "You had to remove every bowl above it first",
        "Only stacks with no bowls could be accessed",
        "Lower bowls vanished after a few seconds",
        1,
        "Stacks only allow access to the top element, "
        + "so bowls above the target must be removed first."
        ),

        Q(
        "In the bowl game, what is the action called when placing a bowl onto a stack?",
        "Pop",
        "Dequeue",
        "Push",
        "Traverse",
        2,
        "Adding an element to the top of a stack is called PUSH."
        ),

        Q(
        "What is the action called when removing the top bowl from a stack?",
        "Push",
        "Peek",
        "Enqueue",
        "Pop",
        3,
        "Removing the top element from a stack is called POP."
        ),

        Q(
        "Why did the game sometimes require using another stack temporarily?",
        "Stacks cannot hold more than one color",
        "Extra stacks help move blocking bowls out of the way",
        "The game forces equal stack sizes",
        "Temporary stacks automatically sort bowls",
        1,
        "Extra stacks act as temporary storage while rearranging bowls."
        ),

        Q(
        "Which bowl could you always access immediately in a stack?",
        "The bottom bowl",
        "The middle bowl",
        "The top bowl",
        "Any bowl chosen randomly",
        2,
        "Stacks are designed for fast access to the top element only."
        ),

        Q(
        "What happened when you successfully moved a bowl "
        + "from one stack to another?",
        "A pop followed by a push operation occurred",
        "A dequeue operation occurred",
        "The entire stack was copied",
        "The stack reset automatically",
        0,
        "Moving a bowl means removing it from one stack (pop) "
        + "and placing it onto another (push)."
        ),
    };

        // -----------------------------------------------------------------
        // MEDIUM
        // Reasoning + consequences + gameplay strategy
        // -----------------------------------------------------------------
        stackBank.mediumQuestions = new List<QuizQuestion>
    {
        Q(
        "Why was moving bowls in the stacks fast and simple?",
        "Because stacks sort themselves automatically",
        "Because stacks only operate on the top element",
        "Because all bowls are accessed together",
        "Because stacks use random access",
        1,
        "Only the top bowl can be accessed, so the game does not need to search through the stack."
        ),

        Q(
        "You needed to get to a bowl buried deep in the stack. "
+ "What did you have to do first?",
        "Move the target bowl directly",
        "Remove bowls above it one by one first",
        "Reverse the entire stack automatically",
        "Duplicate the stack",
        1,
        "You had to clear the bowls above before reaching the target bowl."
        ),

        Q(
        "Why did some players finish the puzzle using fewer moves than others?",
        "Some move sequences caused unnecessary extra moves",
        "The game changes rules randomly",
        "Stacks reorder themselves unpredictably",
        "Only large stacks count toward moves",
        0,
        "Planning ahead helps avoid unnecessary extra moves."
        ),

        Q(
        "In the game, what role did the empty stack usually play?",
        "It permanently stored completed bowls",
        "It acted as temporary storage while rearranging bowls",
        "It prevented overflow",
        "It automatically sorted bowls",
        1,
        "An empty stack is often used as temporary space "
        + "to hold bowls during rearrangement."
        ),

        Q(
        "What would happen if stacks allowed removing bowls "
        + "from the middle directly?",
        "The structure would no longer behave like a stack",
        "Push operations would stop working",
        "Overflow would disappear",
        "The game would become FIFO",
        0,
        "A stack specifically restricts access to the top element."
        ),

        Q(
        "Why was it important to plan your bowl moves carefully?",
        "Because some moves temporarily block future actions",
        "Because stacks move automatically after waiting",
        "Because lower bowls move by themselves",
        "Because stacks change size randomly",
        0,
        "Poor planning can create unnecessary blocking and extra moves."
        ),

        Q(
        "Why was the top bowl always removed first?",
        "The game preserves the order bowls were added in",
        "The stack is designed so the newest bowl is always on top — that's the only one you can grab",
        "Because lower bowls are deleted",
        "Gravity physics pull the top bowl down",
        1,
        "Stacks always remove the most recently added element first."
        ),

        Q(
        "What problem occurred when trying to place a bowl "
        + "onto a full stack?",
        "Traversal error",
        "Stack overflow",
        "Queue underflow",
        "Pointer reassignment",
        1,
        "Adding to a full stack causes overflow."
        ),

        Q(
        "You just made a wrong move and want to undo it. Why does a stack work perfectly for storing your move history?",
        "The game would undo your oldest move first",
        "Your most recent move is always on top to undo first",
        "Stacks let you delete any move randomly",
        "Stacks automatically organize your move history alphabetically",
        1,
        "The most recent move is usually the first one players want to undo."
        ),
    };

        // -----------------------------------------------------------------
        // HARD
        // Application + prediction + deeper understanding
        // -----------------------------------------------------------------
        stackBank.hardQuestions = new List<QuizQuestion>
    {
        Q(
        "You need the GREEN bowl that is at the bottom of a stack. "
        + "What must happen first?",
        "The stack must be duplicated",
        "All bowls above the green bowl must be removed",
        "The bottom bowl automatically rises upward",
        "The stack changes into a queue",
        1,
        "Stacks only allow top access, so all blocking bowls "
        + "must be removed first."
        ),

        Q(
        "Imagine the game removed the OLDEST bowl first instead of the newest. What type of structure would the game behave like?",
        "It would behave like a queue instead of a stack",
        "It would become a linked list",
        "Push operations would disappear",
        "Overflow would no longer occur",
        0,
        "Removing the oldest item first follows FIFO behavior, "
        + "which describes queues."
        ),

        Q(
        "You accidentally filled every stack completely. "
        + "Why could this make the puzzle impossible to continue?",
        "No stack had free space for temporary moves",
        "Stacks automatically delete bowls when full",
        "The game disables pop operations",
        "Overflow reverses the stack order",
        0,
        "Temporary free space is often required for rearranging stacks."
        ),

        Q(
        "Which strategy usually reduces the total number of moves?",
        "Moving bowls randomly",
        "Planning several moves ahead before acting",
        "Always filling one stack completely first",
        "Only moving bowls between two stacks",
        1,
        "Planning ahead helps avoid unnecessary operations and backtracking."
        ),

        Q(
"In some puzzles, making one wrong move forced you to make several extra moves later. Why?",
"Because stacks can become blocked by poorly planned moves",
"Because the game randomly changes stack positions",
"Because bowls automatically move between stacks",
"Because full stacks remove bowls automatically",
0,
"Poor planning can block important bowls and create unnecessary extra moves."
),

        Q(
        "If you removed every bowl from a stack one by one, "
        + "in what order would they come out?",
        "From bottom to top",
        "Randomly",
        "From top to bottom",
        "From smallest to largest",
        2,
        "Stacks remove items in reverse order of insertion."
        ),

        Q(
        "Compared to a shelf where you can grab any bowl directly, what limitation did the stack puzzle have?",
        "Stacks cannot store multiple items",
        "Stacks do not allow fast direct access to middle elements",
        "Stacks cannot remove elements",
        "Stacks require sorting before use",
        1,
        "Stacks only allow direct access to the top bowl, not bowls in the middle."
        ),

        Q(
        "You wasted moves solving the bowl puzzle. What's the consequence?",
        "Your solution becomes less efficient (more moves = lower score)",
        "The game automatically added more bowls to the stack",
        "The stack turned into a queue",
        "All your progress was erased",
        0,
        "Every unnecessary move adds to your total. The most efficient solution uses the fewest moves possible."
        ),

        Q(
        "Why was having an empty stack useful during difficult puzzles?",
        "It gives you temporary workspace to rearrange bowls",
        "It permanently stores completed bowls",
        "It prevents overflow forever",
         "It lets you grab bottom bowls directly",
        0,
        "Temporary storage space is critical for rearranging stack contents."
        ),
    };
    }

    // -------------------------------------------------------------------------
    // QUEUE QUESTIONS  (parking-lot / car-exit game)
    // -------------------------------------------------------------------------
    // -------------------------------------------------------------------------
    // QUEUE QUESTIONS  (parking lot / car exit game)
    // Gameplay focus:
    // - FIFO behaviour
    // - Front/back restrictions
    // - Waiting order
    // - Temporary holding area
    // - Queue overflow
    // - Fair processing order
    // -------------------------------------------------------------------------
    private void PopulateQueueQuestions()
    {
        if (queueBank == null) return;

        // -----------------------------------------------------------------
        // BASIC
        // Recognition + direct gameplay understanding
        // -----------------------------------------------------------------
        queueBank.basicQuestions = new List<QuizQuestion>
    {
        Q(
        "In the parking lot game, why could cars only leave from the FRONT?",
        "Because The game permanently locked cars at the back" ,
        "Because the car that entered first had to leave first",
        "Because the game chooses random cars",
        "Because front cars move faster",
        1,
        "Queues follow FIFO behavior (First In, First Out). "
        + "The car that entered first leaves first."
        ),

        Q(
        "What does FIFO mean in the parking lot game?",
        "Fast In, Fast Out",
        "First In, First Out",
        "Front Input, Final Output",
        "Fixed In, Fixed Out",
        1,
        "FIFO means the first item added is the first one removed."
        ),

        Q(
        "Where did new cars join the queue?",
        "At the front",
        "In the middle",
        "At the back",
        "At random positions",
        2,
        "New elements are added to the BACK of a queue."
        ),

        Q(
        "What is the action called when a new car joins the queue?",
        "Push",
        "Pop",
        "Traverse",
        "Enqueue",
        3,
        "Adding an element to a queue is called ENQUEUE."
        ),

        Q(
        "What is the action called when the front car leaves the queue?",
        "Push",
        "Pop",
        "Dequeue",
        "Insert",
        2,
        "Removing the front element from a queue is called DEQUEUE."
        ),

        Q(
        "Why could you not remove a car directly from the middle of the queue?",
        "Middle cars disappear temporarily",
        "Queues only allow removal from the front",
        "The game hides middle cars",
        "The game permanently locked middle cars" ,
        1,
        "Queues preserve waiting order by only allowing removal "
        + "from the front."
        ),

        Q(
        "A target car was blocked by two cars in front of it. "
        + "What had to happen first?",
        "The target car could jump ahead",
        "The two front cars had to leave first",
        "The queue reversed automatically",
        "The target car moved to holding automatically",
        1,
        "Cars in front must leave before cars behind them can exit."
        ),

        Q(
        "What was the purpose of the holding area in the game?",
        "To permanently remove cars",
        "To temporarily store cars while rearranging the queue",
        "To sort cars automatically",
        "To increase queue speed",
        1,
        "The holding area acted as temporary storage during rearrangement."
        ),

        Q(
        "Which car is always easiest to remove in a queue?",
        "The last car",
        "The middle car",
        "The front car",
        "Any car chosen randomly",
        2,
        "Queues are designed for efficient front removal."
        ),

        Q(
        "What happened when a new car entered the parking lane?",
        "It entered at the back behind all existing cars",
        "It became the front car immediately",
        "It replaced the oldest car",
        "The queue reordered automatically",
        0,
        "Queues maintain arrival order by adding new elements to the back."
        ),
    };

        // -----------------------------------------------------------------
        // MEDIUM
        // Reasoning + consequences + gameplay strategy
        // -----------------------------------------------------------------
        queueBank.mediumQuestions = new List<QuizQuestion>
    {
        Q(
        "Why did waiting cars in the parking lane feel the system was fair?",
        "Cars were processed in the order they arrived",
        "Newer cars got to go first",
        "The lanes automatically sorted cars by color",
        "Cars were selected randomly",
        0,
        "Cars left in the same order they arrived."
        ),

        Q(
        "Why was the holding area useful during difficult parking situations?",
        "It allowed temporary movement of blocking cars",
        "It automatically solved the puzzle",
        "It reversed the queue order",
        "It prevented dequeue operations",
        0,
        "Temporary storage helps rearrange cars without losing order."
        ),

        Q(
        "What would happen if cars could leave from both the front and back?",
        "The structure would no longer behave like a normal queue",
        "The queue would become a stack",
        "FIFO behavior would improve",
        "Enqueue operations would stop working",
        0,
        "A standard queue removes items only from the front."
        ),

        Q(
        "Why did some solutions require many extra moves?",
        "Poor planning forced unnecessary rearrangement",
        "Queues automatically add random cars",
        "Cars changed order by themselves",
        "The holding area deleted cars randomly",
        0,
        "Poor planning caused extra unnecessary car movements."
        ),

        Q(
        "In real life, why do banks and ticket counters use queues instead of letting anyone go next?",
        "Newest customers skip to the front",
        "Queues serve people fairly in the order they arrived",
        "Queues randomly pick who's next",
        "Queues give priority to the smallest person",
        1,
        "Queues preserve fairness by serving the oldest request first."
        ),

        Q(
        "What problem occurred when trying to add a car "
        + "to a completely full queue?",
        "Queue overflow",
        "Stack overflow",
        "Traversal failure",
        "Pointer reassignment",
        0,
        "Adding to a full queue causes queue overflow."
        ),

        Q(
        "What queue behavior did the parking lot game mainly teach?",
        "Last In, First Out",
        "Random access",
        "First In, First Out",
        "Middle-first processing",
        2,
        "Cars exited in the same order they entered."
        ),

        Q(
        "Why was removing the target car sometimes slow?",
        "All cars ahead of it had to leave first",
        "Queues always sort cars before removal",
        "The queue moved backward temporarily",
        "Cars at the front become locked",
        0,
        "Queues require processing earlier items before later ones."
        ),

        Q(
        "Why was adding and removing cars from the queue usually quick?",
        "They only affect the ends of the queue",
        "Queues rearrange all elements automatically",
        "Every operation sorts the queue",
        "Queues duplicate all items before processing",
        0,
        "Cars only enter from the back and leave from the front."
        ),
    };

        // -----------------------------------------------------------------
        // HARD
        // Application + prediction + deeper understanding
        // -----------------------------------------------------------------
        queueBank.hardQuestions = new List<QuizQuestion>
    {
        Q(
        "What would happen if the newest arriving car "
        + "always exited first instead?",
        "The structure would behave like a stack",
        "The queue would become circular",
        "The holding area would disappear",
        "the structure would follow FIFO but in reverse",
        0,
        "Removing the newest item first follows LIFO behavior, "
        + "which describes stacks."
        ),

        Q(
        "Which strategy usually reduces unnecessary car movement?",
        "Moving cars randomly",
        "Planning ahead before using the holding area",
        "Always removing the newest car first",
        "Keeping the queue completely full",
        1,
        "Good planning minimizes unnecessary enqueue and dequeue operations."
        ),

        Q(
        "Compared to a parking area where you could move any car directly, what limitation did the queue lanes have?",
        "Queues cannot store multiple cars",
        "Queues do not allow fast direct access to middle cars",
        "Queues cannot remove cars",
        "Queues require sorting before use",
        1,
        "You could not directly access cars stuck in the middle of the lane."
        ),

        Q(
        "If cars could suddenly skip ahead of older cars, "
        + "What important feature would the game lose?",
        "Fair arrival order",
        "The lane's maximum capacity",
        "Overflow protection",
         "How fast you can traverse the lane",
        0,
        "FIFO fairness depends on preserving arrival order."
        ),

        Q(
"During difficult levels, why was it important to keep some free space in the holding area?",
"Without free space, rearranging cars became much harder",
"Free space automatically sorted the cars",
"The holding area removed cars permanently",
"Cars could only move when the queue was full",
0,
"Temporary free space made it easier to rearrange blocked cars."
),

        Q(
        "What real-world situation most closely matches the parking lot lanes?",
        "People waiting in a line at a ticket counter",
        "Undoing actions in a text editor",
        "Accessing array elements by index",
        "Sorting photos alphabetically",
        0,
        "People in lines are usually served in FIFO order."
        ),

        Q(
        "Moving cars into the holding area sometimes required extra steps. Why was this still useful?",
        "Extra temporary movement adds more operations but enables rearrangement",
        "Holding automatically duplicates cars",
        "Cars become permanently reordered",
        "Queues reverse after every move",
        0,
        "Temporary storage helps solve the puzzle by allowing rearrangement, even though it may add extra operations."
        ),

        Q(
        "The parking lanes taught you one main rule. Which real-life situation follows the SAME rule?",
        "A line of people at an ATM",
        "A stack of dirty plates in a cafeteria",
        "A random number generator",
        "A pile of papers where you read the top one first",
        0,
        "That's FIFO — first in, first out. The car that arrived first leaves first, just like the first person in line gets served first."
        ),
    };
    }

    // -------------------------------------------------------------------------
    // LINKED LIST QUESTIONS  (train-car / connection game)
    // -------------------------------------------------------------------------
    // -------------------------------------------------------------------------
    // LINKED LIST QUESTIONS  (train-car connection game)
    // Gameplay focus:
    // - Nodes and pointers
    // - Head traversal
    // - Reachability
    // - Re-linking connections
    // - Insertion/deletion
    // - Broken chains and cycles
    // -------------------------------------------------------------------------
    private void PopulateLinkedListQuestions()
    {
        if (linkedListBank == null) return;

        // -----------------------------------------------------------------
        // BASIC
        // Recognition + direct gameplay understanding
        // -----------------------------------------------------------------
        linkedListBank.basicQuestions = new List<QuizQuestion>
    {
        Q(
        "In the game, what did the arrow between train cars represent?",
        "The speed of the train",
        "A connection to the next train car",
        "The color of the train car",
        "The train's direction",
        1,
        "In a linked list, each node stores a reference "
        + "to the next node."
        ),

        Q(
        "In the train game, what did the locomotive (HEAD) represent?",
        "The largest node",
        "The final node in the list",
        "The first train car that connects to the rest of the train",
        "A node with two pointers",
        2,
        "The head is the first node and acts as the entry point "
        + "to the entire list."
        ),

        Q(
        "What happened when a train car lost all connections to the train?",
        "It automatically moved to the front",
        "It became unreachable and disappeared",
        "It connected itself randomly",
        "It reversed the train direction",
        1,
        "A disconnected train car could no longer be reached from the locomotive so it gets removed."
        ),

        Q(
        "What were you changing when you dragged an arrow from one train car to another?",
        "Sorting the train",
        "Changing which train car another car connects to",
        "Duplicating a node",
        "Deleting the list",
        1,
        "You were updating which node another node points to."
        ),

        Q(
        "What did each train car in the linked-list game contain?",
        "Only an arrow",
        "Only a number",
        "Data and a connection to the next train car",
        "The entire train",
        2,
        "A linked-list node typically stores both data "
        + "and a pointer/reference."
        ),

        Q(
        "What happened when you correctly connected two train cars?",
        "A new connection between train cars was created",
        "The entire train reversed",
        "A queue operation occurred",
        "The head node disappeared",
        0,
        "Updating pointers changes the structure of the linked list."
        ),

        Q(
        "Why is the HEAD node (represented by the train locomotive) important?",
        "Without it, the list may become unreachable",
        "It stores every node's data",
        "It prevents overflow",
        "It automatically sorts the list",
        0,
        "The head is the entry point to the list."
        ),

        Q(
        "What did traversal mean in the train game?",
        "Sorting all nodes",
        "Following the train cars one by one using the arrows",
        "Duplicating nodes",
        "Deleting unreachable nodes",
        1,
        "Traversal means following node references step by step."
        ),

         Q(
        "What did the last train car's arrow point to at the beginning?",
        "Back to the locomotive",
        "Nothing",
        "The middle car",
        "The next car in line",
        1,
        "The last node in a linked list points to null, indicating the end of the list."
        ),
    };

        // -----------------------------------------------------------------
        // MEDIUM
        // Reasoning + consequences + gameplay strategy
        // -----------------------------------------------------------------
        linkedListBank.mediumQuestions = new List<QuizQuestion>
    {
        Q(
        "Why was reconnecting train cars important in the game?",
        "Wrong connections could break the train structure",
        "Pointers automatically repair themselves",
        "Train cars only store colors",
        "Traversal works without connections",
        0,
        "A linked list depends entirely on correct node references."
        ),

        Q(
        "Why did disconnected train cars disappear?",
        "Unreachable cars can no longer be accessed from the head",
        "Disconnected cars become the new head automatically",
        "Pointers reverse automatically",
        "The game randomly removes cars",
        0,
        "A disconnected train car could no longer be reached from the locomotive so it gets removed."
        ),

        Q(
        "You want to add more train cars to your linked list. Why is this so easy compared to a numbered parking spot system?",
        "You can connect new cars without moving everything else",
        "The list automatically doubles in size",
        "Every car stores all other cars' locations",
        "Pointers remove memory limits",
        0,
        "You can point the existing car to your new car, No need to shuffle or renumber anything."
        ),

        Q(
        "What problem could occur if you accidentally changed "
        + "the wrong connection?",
        "Part of the train could become unreachable",
        "The train automatically fixes itself",
        "Overflow occurs immediately",
        "All nodes become duplicated",
        0,
        "Incorrect pointer updates can disconnect nodes."
        ),

        Q(
"Why was reconnecting train cars in the correct order important?",
"Wrong connections could disconnect parts of the train",
"Train cars automatically fixed broken links",
"The locomotive automatically repaired pointers",
"Traversal works even with broken connections",
0,
"Incorrect connection order could make parts of the train unreachable."
),

        Q(
        "Why was adding new train cars easier in the linked-list structure?",
        "Train cars can be inserted without shifting all other elements",
        "Train cars automatically sort themselves",
        "Traversing the train cars becomes constant time O(1)",
        "Lists cannot contain duplicates",
        0,
        "Insertion usually only requires pointer changes."
        ),

        Q(
        "What happens if the HEAD (locomotive) reference is lost?",
        "Only the last train car disappears",
        "The entire train may become unreachable",
        "The list converts into a queue",
        "Traversal becomes faster",
        1,
        "Without the head, there is no entry point into the list."
        ),

        Q(
        "You're about to reconnect some arrows between train cars. Why do you need to plan your clicks carefully?",
        "Changing arrows in the wrong order can make you lose access to some cars forever",
        "Arrows automatically fix themselves",
        "Traversal becomes impossible after any change",
        "Once changed, cars can never reconnect",
        0,
        "If you point a car somewhere else before saving its connection, that old path is gone"
        ),
    };

        // -----------------------------------------------------------------
        // HARD
        // Application + prediction + deeper understanding
        // -----------------------------------------------------------------
        linkedListBank.hardQuestions = new List<QuizQuestion>
    {
        Q(
        "What would happen if a train car pointed back "
        + "to an earlier car in the train?",
        "Traversal could loop forever",
        "The train would automatically sort itself",
        "The head train car would disappear",
        "Traversal would become faster",
        0,
        "This creates a cycle in the linked list."
        ),

        Q(
        "Compared to numbered train cars that could be accessed instantly, what limitation did the linked-list train have?",
        "You had to move through the train one car at a time",
        "Linked lists cannot grow longer",
        "Train cars cannot store any data",
        "Arrows prevent you from adding new cars",
        0,
        "You had to follow the train connections step by step to reach a specific car."
        ),

        Q(
        "Why could reconnecting train cars in the wrong order break the train?",
        "Important connections might get lost permanently",
        "Arrows automatically reverse direction to fix mistakes",
        "The locomotive makes a copy of itself",
        "Traversal becomes completely random",
        0,
        "If you point a car somewhere else before securing its old connection, you might lose access to cars that " +
        "were only reachable through it"
        ),

        Q(
        "You need to insert a new train car between two existing cars. Why is a linked list perfect for this?",
        "You can add and remove cars without moving all the others ",
        "Linked lists automatically sort themselves alphabetically",
        "Arrows make traversal faster",
        "Linked lists always use less memory than any alternative",
        0,
        "No shuffling, Just point the car before to your new car, and your new car to the car after."
        ),

        Q(
        "What would happen if every train car stopped pointing to the next car?",
        "Traversal would stop after each node",
        "The list would reverse automatically",
        "The list would become circular",
        "Nodes would automatically reconnect",
        0,
        "Null marks the end of a linked list."
        ),

        Q(
        "Why did the game visually demonstrate linked lists well?",
        "The train cars and arrows made node connections and traversal visible",
        "Linked lists depend on colors",
        "Traversal is impossible without graphics",
        "Pointers only exist in games",
        0,
        "The game made node connections and traversal visible."
        ),

        Q(
        "What did disconnected train cars represent in programming?",
        "Memory that can no longer be accessed",
        "Queue overflow",
        "Array indexing",
        "Recursive sorting",
        0,
        "A node with no pointer pointing at it becomes unreachable and gets garbage collected."
        ),

        Q(
        "In the train game, what would happen if you accidentally connected a train car to point back to an earlier car, creating a loop?",
        "You would follow the arrows in a circle forever, never reaching the end",
        "Train cars would automatically disappear",
        "The train would convert into a parking lot queue",
        "All arrows would stop working completely",
        0,
        "A cycle creates an infinite loop — traversal would never reach the end of the train"
        ),
    };
    }

    // -------------------------------------------------------------------------
    // Helper — creates a QuizQuestion cleanly
    // -------------------------------------------------------------------------
    private static QuizQuestion Q(
        string question,
        string a, string b, string c, string d,
        int correctIndex,
        string explanation)
    {
        return new QuizQuestion
        {
            question = question,
            options = new[] { a, b, c, d },
            correctAnswerIndex = correctIndex,
            explanation = explanation
        };
    }
}