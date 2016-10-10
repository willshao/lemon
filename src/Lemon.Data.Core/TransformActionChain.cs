﻿using System;
using System.Collections.Generic;

namespace Lemon.Data.Core
{
    public class TransformActionChain<TSource>
    {
        private Node _current;

        public TransformActionChain(Node current)
        {
            _current = current;
        }

        public TransformActionChain<TTarget> Transform<TTarget>(Func<TSource, TTarget> func)
        {
            var transformNode = new TransformNode<TSource, TTarget>
            {
                Block = func
            };

            if (_current.NodeType == NodeType.SourceNode ||
                _current.NodeType == NodeType.TransformNode ||
                _current.NodeType == NodeType.TransformManyNode
                )
            {
                (_current as ISource).Next = transformNode;
                transformNode.Prev = _current;
            }
            else if (_current.NodeType == NodeType.BroadCastNode)
            {
                (_current as IBroadCast).AddChild(transformNode);
                transformNode.Prev = _current;
            }
            else
            {
                throw new Exception("Node type does not support next");
            }

            return new TransformActionChain<TTarget>(transformNode);
        }

        public TransformActionChain<TTarget> TransformMany<TTarget>(Func<TSource, IEnumerable<TTarget>> expression)
        {
            var node = new TransformManyNode<TSource, TTarget> { Block = expression };

            if (_current.NodeType == NodeType.SourceNode ||
                _current.NodeType == NodeType.TransformNode ||
                _current.NodeType == NodeType.TransformManyNode
                )
            {
                (_current as ISource).Next = node;
                node.Prev = _current;
            }
            else if (_current.NodeType == NodeType.BroadCastNode)
            {
                (_current as IBroadCast).AddChild(node);
                node.Prev = _current;
            }
            else
            {
                throw new Exception("Node type does not support next");
            }

            return new TransformActionChain<TTarget>(node);
        }

        public TransformActionChain<TTarget> TransformMany<TTarget>(ITransformManyBlock<TSource, TTarget> block)
        {
            return TransformMany(block.Transform);
        }

        public TransformActionChain<TTarget> Transform<TTarget>(ITransformBlock<TSource, TTarget> block)
        {
            return Transform(block.Transform);
        }

        public BroadCastActionChain<TSource> Broadcast()
        {
            var broadCastNode = new BroadCastNode<TSource>();

            if (_current.NodeType == NodeType.SourceNode || 
                _current.NodeType == NodeType.TransformNode ||
                _current.NodeType == NodeType.TransformManyNode)
            {
                (_current as ISource).Next = broadCastNode;
                broadCastNode.Prev = _current;
            }
            else
            {
                throw new Exception("Node type does not support broadcast");
            }

            return new BroadCastActionChain<TSource>(broadCastNode);
        }

        public void Output(IDataWriter<TSource> writer)
        {
            var actionNode = new ActionNode<TSource>
            {
                Write = writer.Write
            };

            if (_current.NodeType == NodeType.SourceNode ||
                _current.NodeType == NodeType.TransformNode ||
                _current.NodeType == NodeType.TransformManyNode)
            {
                (_current as ISource).Next = actionNode;
            }
            else if (_current.NodeType == NodeType.BroadCastNode)
            {
                (_current as IBroadCast).AddChild(actionNode);
            }
            else
            {
                throw new Exception("Node type does not support output");
            }
        }
    }
}
