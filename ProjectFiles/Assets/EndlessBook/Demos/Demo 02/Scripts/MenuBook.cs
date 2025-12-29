namespace echo17.EndlessBook.Demo02
{
    using echo17.EndlessBook;
    using System;
    using System.Linq;
    using UnityEngine;

    public enum BookActionTypeEnum
    {
        ChangeState,
        TurnPage
    }

    public delegate void BookActionDelegate(BookActionTypeEnum actionType, int actionValue);

    public class MenuBook : MonoBehaviour
    {
        public Action OnBackClosed;
        protected bool audioOn = false;
        protected bool flipping = false;

        public EndlessBook book;
        public float openCloseTime = 0.3f;
        public EndlessBook.PageTurnTimeTypeEnum groupPageTurnType;
        public float singlePageTurnTime;
        public float groupPageTurnTime;
        public int tableOfContentsPageNumber;

        [SerializeField] AudioClip bookOpenSound,
        bookCloseSound,
        pageTurnSound,
        pagesFlippingSound;
        new AudioSource audio;

        public float pagesFlippingSoundDelay;
        public TouchPad touchPad;
        public PageView[] pageViews;

        void Start()
        {
            audio = GetComponent<AudioSource>();
            // turn off all the mini-scenes since no pages are visible
            TurnOffAllPageViews();

            // set up touch pad handlers
            touchPad.touchDownDetected = TouchPadTouchDownDetected;
            touchPad.touchUpDetected = TouchPadTouchUpDetected;
            touchPad.tableOfContentsDetected = TableOfContentsDetected;
            touchPad.dragDetected = TouchPadDragDetected;

            // set the book closed
            OnBookStateChanged(EndlessBook.StateEnum.ClosedFront, EndlessBook.StateEnum.ClosedFront, -1);

            // turn on the audio now that the book state is set the first time,
            // otherwise we'd hear a noise and no change would occur
            audioOn = true;
        }

        public void OpenPage(int page)
        {
            TurnToPage(page);
            /*for(int i = 0; i < pageViews.Length; i++)
            {
                if(page.GetType() == pageViews[i].GetType())
                {
                    TurnToPage(i);
                    break;
                }
            }*/
        }

        protected virtual void OnBookStateChanged(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int pageNumber)
        {
            switch (toState)
            {
                case EndlessBook.StateEnum.ClosedFront:
                    break;
                case EndlessBook.StateEnum.ClosedBack:
                    // play the closed sound
                    if (audioOn)
                    {
                        audio.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                        audio.PlayOneShot(bookCloseSound);
                    }

                    // turn off page mini-scenes
                    TurnOffAllPageViews();
                    OnBackClosed?.Invoke();
                    break;

                case EndlessBook.StateEnum.OpenMiddle:

                    if (fromState != EndlessBook.StateEnum.OpenMiddle)
                    {
                        // play open sound
                        audio.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                        audio.PlayOneShot(bookOpenSound);
                    }
                    else
                    {
                        // stop the flipping sound
                        flipping = false;
                        audio.Stop();
                    }

                    // turn off the front and back page mini-scenes
                    TogglePageView(0, false);
                    TogglePageView(999, false);

                    break;

                case EndlessBook.StateEnum.OpenFront:
                case EndlessBook.StateEnum.OpenBack:

                    // play the open sound
                    audio.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                    audio.PlayOneShot(bookOpenSound);
                    break;
            }

            // turn on the touchpad
            ToggleTouchPad(true);
        }

        protected virtual void ToggleTouchPad(bool on)
        {
            // left page should only be available if the book is not in the ClosedFront state
            touchPad.Toggle(TouchPad.PageEnum.Left, on && book.CurrentState != EndlessBook.StateEnum.ClosedFront);

            // right page should only be available if the book is not in the ClosedBack state
            touchPad.Toggle(TouchPad.PageEnum.Right, on && book.CurrentState != EndlessBook.StateEnum.ClosedBack);

            // only use the table of contents "button" if not on the first group of pages
            touchPad.ToggleTableOfContents(on && book.CurrentLeftPageNumber > 1);
        }

        protected virtual void TurnOffAllPageViews()
        {
            for (var i = 0; i < pageViews.Length; i++)
            {
                if (pageViews[i] != null)
                {
                    pageViews[i].Deactivate();
                }
            }
        }

        protected virtual void TogglePageView(int pageNumber, bool on)
        {
            var pageView = GetPageView(pageNumber);

            if (pageView != null)
            {
                if (pageView != null)
                {
                    if (on)
                    {
                        pageView.Activate();
                    }
                    else
                    {
                        pageView.Deactivate();
                    }
                }
            }
        }

        protected virtual void OnPageTurnStart(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            // play page turn sound if not flipping through multiple pages
            if (!flipping)
            {
                audio.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audio.PlayOneShot(pageTurnSound);
            }

            // turn off the touch pad
            ToggleTouchPad(false);

            // turn on the front and back page views of the page if necessary
            TogglePageView(pageNumberFront, true);
            TogglePageView(pageNumberBack, true);

            switch (turnDirection)
            {
                case Page.TurnDirectionEnum.TurnForward:

                    // turn on the last visible page view if necessary
                    TogglePageView(pageNumberLastVisible, true);

                    break;

                case Page.TurnDirectionEnum.TurnBackward:

                    // turn on the first visible page view if necessary
                    TogglePageView(pageNumberFirstVisible, true);

                    break;
            }
        }

        protected virtual void OnPageTurnEnd(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            switch (turnDirection)
            {
                case Page.TurnDirectionEnum.TurnForward:

                    // turn off the two pages that are now hidden by this page
                    TogglePageView(pageNumberFirstVisible - 1, false);
                    TogglePageView(pageNumberFirstVisible - 2, false);

                    break;

                case Page.TurnDirectionEnum.TurnBackward:

                    // turn off the two pages that are now hidden by this page
                    TogglePageView(pageNumberLastVisible + 1, false);
                    TogglePageView(pageNumberLastVisible + 2, false);

                    break;
            }
        }

        protected virtual void TableOfContentsDetected()
        {
            TurnToPage(tableOfContentsPageNumber);
        }

        protected virtual void TouchPadTouchDownDetected(TouchPad.PageEnum page, Vector2 hitPointNormalized)
        {
            if (book.CurrentState == EndlessBook.StateEnum.OpenMiddle)
            {
                PageView pageView;

                switch (page)
                {
                    case TouchPad.PageEnum.Left:

                        // get the left page view if available
                        pageView = GetPageView(book.CurrentLeftPageNumber);

                        if (pageView != null)
                        {
                            // call touchdown on the page view
                            pageView.TouchDown();
                        }

                        break;

                    case TouchPad.PageEnum.Right:

                        // get the right page view if available
                        pageView = GetPageView(book.CurrentRightPageNumber);

                        if (pageView != null)
                        {
                            // call the touchdown on the page view
                            pageView.TouchDown();
                        }

                        break;
                }
            }
        }

        protected virtual void TouchPadTouchUpDetected(TouchPad.PageEnum page, Vector2 hitPointNormalized, bool dragging)
        {
            switch (book.CurrentState)
            {
                case EndlessBook.StateEnum.ClosedFront:

                    switch (page)
                    {
                        case TouchPad.PageEnum.Right:

                            // transition from the ClosedFront to the OpenFront states
                            OpenFront();

                            break;
                    }

                    break;

                case EndlessBook.StateEnum.OpenFront:

                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:

                            // transition from the OpenFront to the ClosedFront states
                            ClosedFront();

                            break;

                        case TouchPad.PageEnum.Right:

                            // transition from the OpenFront to the OpenMiddle states
                            OpenMiddle();

                            break;
                    }

                    break;

                case EndlessBook.StateEnum.OpenMiddle:

                    PageView pageView;

                    if (dragging)
                    {
                        // get the left page view if available.
                        // in this demo we only have one group of pages that handle the drag: the map.
                        // instead of having logic for dragging on both pages, we'll just handle it on the left
                        pageView = GetPageView(book.CurrentLeftPageNumber);

                        if (pageView != null)
                        {
                            // call the drag method on the page view
                            pageView.Drag(Vector2.zero, true);
                        }

                        return;
                    }

                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:

                            // get the left page view if available
                            pageView = GetPageView(book.CurrentLeftPageNumber);

                            if (pageView != null)
                            {
                                // cast a ray into the page and exit if we hit something (don't turn the page)
                                if (pageView.RayCast(hitPointNormalized, BookAction))
                                {
                                    return;
                                }
                            }

                            break;

                        case TouchPad.PageEnum.Right:

                            // get the right page view if available
                            pageView = GetPageView(book.CurrentRightPageNumber);

                            if (pageView != null)
                            {
                                // cast a ray into the page and exit if we hit something (don't turn the page)
                                if (pageView.RayCast(hitPointNormalized, BookAction))
                                {
                                    return;
                                }
                            }

                            break;
                    }

                    break;

                case EndlessBook.StateEnum.OpenBack:

                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:

                            // transition from the OpenBack to the OpenMiddle states
                            OpenMiddle();

                            break;

                        case TouchPad.PageEnum.Right:

                            // transition from the OpenBack to the ClosedBack states
                            ClosedBack();

                            break;
                    }

                    break;

                case EndlessBook.StateEnum.ClosedBack:

                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:

                            // transition from the ClosedBack to the OpenBack states
                            OpenBack();

                            break;
                    }

                    break;

            }

            switch (page)
            {
                case TouchPad.PageEnum.Left:

                    if (book.CurrentLeftPageNumber == 1)
                    {
                        // if on the first page, transition from the OpenMiddle to the OpenFront states
                        OpenFront();
                    }
                    else
                    {
                        // not on the first page, so just turn back one page
                        book.TurnBackward(singlePageTurnTime, onCompleted: OnBookStateChanged, onPageTurnStart: OnPageTurnStart, onPageTurnEnd: OnPageTurnEnd);
                    }

                    break;

                case TouchPad.PageEnum.Right:

                    if (book.CurrentRightPageNumber == book.LastPageNumber)
                    {
                        // if on the last page, transition from the OpenMiddle to the OpenBack states
                        OpenBack();
                    }
                    else
                    {
                        // not on the last page, so just turn forward a page
                        book.TurnForward(singlePageTurnTime, onCompleted: OnBookStateChanged, onPageTurnStart: OnPageTurnStart, onPageTurnEnd: OnPageTurnEnd);
                    }

                    break;
            }
        }

        protected virtual void TouchPadDragDetected(TouchPad.PageEnum page, Vector2 touchDownPosition, Vector2 currentPosition, Vector2 incrementalChange)
        {
            // only handle drag in the OpenMiddle state
            if (book.CurrentState == EndlessBook.StateEnum.OpenMiddle)
            {
                // get the page view if available
                var pageView = GetPageView(book.CurrentLeftPageNumber);

                if (pageView != null)
                {
                    // drag
                    pageView.Drag(incrementalChange, false);
                }
            }
        }

        protected virtual void BookAction(BookActionTypeEnum actionType, int actionValue)
        {
            switch (actionType)
            {
                case BookActionTypeEnum.ChangeState:

                    // set the book state
                    SetState((EndlessBook.StateEnum)System.Convert.ToInt16(actionValue));

                    break;

                case BookActionTypeEnum.TurnPage:

                    // table of contents actions

                    if (actionValue == 999)
                    {
                        // go to the back page (OpenBack state)
                        OpenBack();
                    }
                    else
                    {
                        // turn to a page
                        TurnToPage(System.Convert.ToInt16(actionValue));
                    }

                    break;
            }
        }

        protected virtual PageView GetPageView(int pageNumber)
        {
            // search for a page view.
            // 0 = front page,
            // 999 = back page
            return pageViews.Where(x => x.name == string.Format("PageView_{0}", (pageNumber == 0 ? "Front" : (pageNumber == 999 ? "Back" : pageNumber.ToString("00"))))).FirstOrDefault();
        }

        public virtual void ClosedFront()
        {
            SetState(EndlessBook.StateEnum.ClosedFront);
        }

        protected virtual void OpenFront()
        {
            // toggle the front page view
            TogglePageView(0, true);

            SetState(EndlessBook.StateEnum.OpenFront);
        }

        protected virtual void OpenMiddle()
        {
            // toggle the left and right page views
            TogglePageView(book.CurrentLeftPageNumber, true);
            TogglePageView(book.CurrentRightPageNumber, true);

            SetState(EndlessBook.StateEnum.OpenMiddle);
        }

        protected virtual void OpenBack()
        {
            // toggle the back page view
            TogglePageView(999, true);

            SetState(EndlessBook.StateEnum.OpenBack);
        }

        protected virtual void ClosedBack()
        {
            SetState(EndlessBook.StateEnum.ClosedBack);
        }

        protected virtual void SetState(EndlessBook.StateEnum state)
        {
            // turn of the touch pad
            ToggleTouchPad(false);

            // set the state
            book.SetState(state, openCloseTime, OnBookStateChanged);
        }

        protected virtual void TurnToPage(int pageNumber)
        {
            var newLeftPageNumber = pageNumber % 2 == 0 ? pageNumber - 1 : pageNumber;

            // play the flipping sound if more than a single page is turning
            if (Mathf.Abs(newLeftPageNumber - book.CurrentLeftPageNumber) > 2)
            {
                flipping = true;
                audio.clip = pagesFlippingSound;
                audio.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audio.PlayDelayed(pagesFlippingSoundDelay);
            }

            // turn to page
            book.TurnToPage(pageNumber, groupPageTurnType, groupPageTurnTime,
                            openTime: openCloseTime,
                            onCompleted: OnBookStateChanged,
                            onPageTurnStart: OnPageTurnStart,
                            onPageTurnEnd: OnPageTurnEnd);
        }
    }
}