declare namespace JSX {
    interface IntrinsicElements extends Element {
        [elemName: string]: MaquetteElementProperties;
    }
    interface Element {
    }
    interface MaquetteElementProperties {
        // copied from maquette.d.ts
        /**
     * The animation to perform when this node is added to an already existing parent.
     * When this value is a string, you must pass a `projectionOptions.transitions` object when creating the
     * projector using [[createProjector]].
     * {@link http://maquettejs.org/docs/animations.html|More about animations}.
     * @param element - Element that was just added to the DOM.
     * @param properties - The properties object that was supplied to the [[h]] method
     */
        enterAnimation?: ((element: Element, properties?: MaquetteElementProperties) => void) | string;
        /**
         * The animation to perform when this node is removed while its parent remains.
         * When this value is a string, you must pass a `projectionOptions.transitions` object when creating the projector using [[createProjector]].
         * {@link http://maquettejs.org/docs/animations.html|More about animations}.
         * @param element - Element that ought to be removed from to the DOM.
         * @param removeElement - Function that removes the element from the DOM.
         * This argument is provided purely for convenience.
         * You may use this function to remove the element when the animation is done.
         * @param properties - The properties object that was supplied to the [[h]] method that rendered this [[VNode]] the previous time.
         */
        exitAnimation?: ((element: Element, removeElement: () => void, properties?: MaquetteElementProperties) => void) | string;
        /**
         * The animation to perform when the properties of this node change.
         * This also includes attributes, styles, css classes. This callback is also invoked when node contains only text and that text changes.
         * {@link http://maquettejs.org/docs/animations.html|More about animations}.
         * @param element - Element that was modified in the DOM.
         * @param properties - The last properties object that was supplied to the [[h]] method
         * @param previousProperties - The previous properties object that was supplied to the [[h]] method
         */
        updateAnimation?: (element: Element, properties?: MaquetteElementProperties, previousProperties?: MaquetteElementProperties) => void;
        /**
         * Callback that is executed after this node is added to the DOM. Child nodes and properties have
         * already been applied.
         * @param element - The element that was added to the DOM.
         * @param projectionOptions - The projection options that were used, see [[createProjector]].
         * @param vnodeSelector - The selector passed to the [[h]] function.
         * @param properties - The properties passed to the [[h]] function.
         * @param children - The children that were created.
         */
        afterCreate?(element: Element, projectionOptions: any, vnodeSelector: string, properties: MaquetteElementProperties, children: Element[]): void;
        /**
         * Callback that is executed every time this node may have been updated. Child nodes and properties
         * have already been updated.
         * @param element - The element that may have been updated in the DOM.
         * @param projectionOptions - The projection options that were used, see [[createProjector]].
         * @param vnodeSelector - The selector passed to the [[h]] function.
         * @param properties - The properties passed to the [[h]] function.
         * @param children - The children for this node.
         */
        afterUpdate?(element: Element, projectionOptions: any, vnodeSelector: string, properties: MaquetteElementProperties, children: Element[]): void;
        /**
         * When specified, the event handlers will be invoked with 'this' pointing to the value.
         * This is useful when using the prototype/class based implementation of Components.
         *
         * When no [[key]] is present, this object is also used to uniquely identify a DOM node.
         */
        readonly bind?: Object;
        /**
         * Used to uniquely identify a DOM node among siblings.
         * A key is required when there are more children with the same selector and these children are added or removed dynamically.
         * NOTE: this does not have to be a string or number, a [[Component]] Object for instance is also possible.
         */
        readonly key?: Object;
        /**
         * An object literal like `{important:true}` which allows css classes, like `important` to be added and removed
         * dynamically.
         */
        readonly classes?: {
            [index: string]: boolean | null | undefined;
        };
        /**
         * An object literal like `{height:'100px'}` which allows styles to be changed dynamically. All values must be strings.
         */
        readonly styles?: {
            [index: string]: string | null | undefined;
        };
        ontouchcancel?(ev: TouchEvent): boolean | void;
        ontouchend?(ev: TouchEvent): boolean | void;
        ontouchmove?(ev: TouchEvent): boolean | void;
        ontouchstart?(ev: TouchEvent): boolean | void;
        readonly action?: string;
        readonly encoding?: string;
        readonly enctype?: string;
        readonly method?: string;
        readonly name?: string;
        readonly target?: string;
        readonly href?: string;
        readonly rel?: string;
        onblur?(ev: FocusEvent): boolean | void;
        onchange?(ev: Event): boolean | void;
        onclick?(ev: MouseEvent): boolean | void;
        ondblclick?(ev: MouseEvent): boolean | void;
        onfocus?(ev: FocusEvent): boolean | void;
        oninput?(ev: Event): boolean | void;
        onkeydown?(ev: KeyboardEvent): boolean | void;
        onkeypress?(ev: KeyboardEvent): boolean | void;
        onkeyup?(ev: KeyboardEvent): boolean | void;
        onload?(ev: Event): boolean | void;
        onmousedown?(ev: MouseEvent): boolean | void;
        onmouseenter?(ev: MouseEvent): boolean | void;
        onmouseleave?(ev: MouseEvent): boolean | void;
        onmousemove?(ev: MouseEvent): boolean | void;
        onmouseout?(ev: MouseEvent): boolean | void;
        onmouseover?(ev: MouseEvent): boolean | void;
        onmouseup?(ev: MouseEvent): boolean | void;
        onmousewheel?(ev: WheelEvent | MouseWheelEvent): boolean | void;
        onscroll?(ev: UIEvent): boolean | void;
        onsubmit?(ev: Event): boolean | void;
        readonly spellcheck?: boolean;
        readonly tabIndex?: number;
        readonly disabled?: boolean;
        readonly title?: string;
        readonly accessKey?: string;
        readonly id?: string;
        readonly type?: string;
        readonly autocomplete?: string;
        readonly checked?: boolean;
        readonly placeholder?: string;
        readonly readOnly?: boolean;
        readonly src?: string;
        readonly value?: string;
        readonly alt?: string;
        readonly srcset?: string;
        /**
         * Puts a non-interactive string of html inside the DOM node.
         *
         * Note: if you use innerHTML, maquette cannot protect you from XSS vulnerabilities and you must make sure that the innerHTML value is safe.
         */
        readonly innerHTML?: string;
        /**
         * Everything that is not explicitly listed (properties and attributes that are either uncommon or custom).
         */
        readonly [index: string]: any;
    }
}